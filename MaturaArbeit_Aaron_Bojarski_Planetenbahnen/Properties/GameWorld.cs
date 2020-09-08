using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Numerics;
using MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties;


namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties
{
    public class GameWorld
    {
        //Die folgende Region ist für die Erstellung einer neuen "Welt" verantwortlich
        #region Create new World
        public GameWorld()
        {
            CelestrialBodies = new List<CelestrialBody>();
            GameTimer = new Stopwatch();
            GameTimer.Start();
            main = App.Current.MainWindow as MainWindow;
        }

        public List<CelestrialBody> CelestrialBodies { get; set; } // in dieser Liste sind alle Körper gespeichert

        public Stopwatch GameTimer { get; }
        private TimeSpan previousGameTick;
        public double ElapsedMillisecondsForOneTick;

        #endregion

        //Die Region "AddBody" fügt einen Körper der Liste "CelestrialBodies" hinzu. Auch die ComboBoxen (Drop-Down) werden aktualisiert.
        #region AddBody
        MainWindow main;
        public void AddCelestrialBody(CelestrialBody body)
        {
            CelestrialBodies.Add(body);
            main.cbbCenteredBody.Items.Add(body.Name);
            main.cbbSelectedBody.Items.Add(body.Name);
            CalcForceForeachBody();
        }
        #endregion

        //Die Region "Drawing" regelt das Zeichnen aller Körper
        #region Drawing
        public int RadiusMultiplicator = Settings.Default.RadiusMultiplicator;
        public int RadiusSummand = Settings.Default.RadiusSummand;
        public CelestrialBody Centeredbody = new CelestrialBody(); //Der zentrierte Körper

        //Die DrawWorld Funktion zeichnet jeden einzelnen Körper nacheinander. Dabei wird unterschieden, ob die Z-Kooridinate benutzt wird oder nicht.
        public void DrawWorld(WriteableBitmap surface, int width, int height, double ZoomFaktor)
        {
            //Wird die Z-Kooridinate benutzt, so ist die Position sicher nicht genau 0. Dann wird folgender Code ausgeführt.
            if(CelestrialBodies.Count <9 && CelestrialBodies[0].Position.Z!=0)
            {
                SortCelestrielbodies(); //Sortiert die Körper nach ihrer Z-Koordinate. Der Vorderste Körper wird nun auch zu vorderst gezeichnet.
                for (int i = 0; i < CelestrialBodies.Count; i++)
                {
                    CurrentCelestrielBodyOrder[i].DrawSingleBody(surface, width, height, ZoomFaktor, Centeredbody, RadiusMultiplicator, RadiusSummand);
                }
            }
            //Ohne Z-Koordinate werden die Körper nach der Reihenfolge der Liste gezeichnet.
            else
            {
                foreach (CelestrialBody body in CelestrialBodies)
                {
                    body.DrawSingleBody(surface, width, height, ZoomFaktor, Centeredbody, RadiusMultiplicator, RadiusSummand);
                }
            }
            
            //die folgenden beiden Linien sind dazu da, um testen zu können ob genug Rechenleistung vorhanden ist. Wird ein Bild alle 16.7 ms gezeichnet, ist dies der Fall.
            ElapsedMillisecondsForOneTick = (GameTimer.Elapsed - previousGameTick).TotalMilliseconds;
            previousGameTick = GameTimer.Elapsed;
        }

        //Die Region "Sort Celestrial-Bodies for drawing" sortiert die Körper so, das der Körper der am weitesten weg ist, als letstes gezeichnet wird. Sie werden in einem Array sortiert.
        #region Sort Celestrial-Bodies for drawing
        CelestrialBody[] CurrentCelestrielBodyOrder = new CelestrialBody[10];
        private void SortCelestrielbodies()
        {
            for (int i = 0; i < CelestrialBodies.Count; i++)
            {
                CurrentCelestrielBodyOrder[i] = null;
            }
            foreach (CelestrialBody body in CelestrialBodies)
            {
                AddNumberToArray(body);
            }
        }
        private void AddNumberToArray(CelestrialBody body)
        {
            int index = GetIndexToInsert(body.Position.Z);
            MoveNumbers(index);
            CurrentCelestrielBodyOrder[index] = body;
        }
        private int GetIndexToInsert(double number)
        {
            for (int i = 0; i < CurrentCelestrielBodyOrder.Length; i++)
            {
                if (CurrentCelestrielBodyOrder[i] == null)
                    return i;
                if (CurrentCelestrielBodyOrder[i].Position.Z <= number)
                    return i;
            }
            return CurrentCelestrielBodyOrder.Length - 1;
        }
        private void MoveNumbers(int fromIndex)
        {
            for (int i = CurrentCelestrielBodyOrder.Length - 1; i > fromIndex; i--)
            {
                CurrentCelestrielBodyOrder[i] = CurrentCelestrielBodyOrder[i - 1];
            }
        }
        #endregion
        #endregion

        //Die Region "Calculation" regelt die Beziehungen zwischen den Körpern und initiiert die Berechnung der neuen Position der Körper
        #region Calculation
        public double CalculatedTime;
        public void GameTick(double Δt)
        {
            CalculatedTime += Δt; //Berechnungszeit wird erhöht. Diese wird dem Benutzter ausgegeben.

            //Die neue Position jedes Körpers wird berechnet
            foreach (var body in CelestrialBodies)
            {
                body.PositionCalculation(Δt);
            }
            CalcForceForeachBody(); //Berechnet die Kräfte, die auf jeden Körper wirken und weist sie dem jeweiligen zu.

            //Die neue Geschwindigkeit jedes Körpers wird berechnet
            foreach (var body in CelestrialBodies)
            {
                body.VelocityCalculation(Δt);
            }

        }


        public void CalcForceForeachBody()
        {
            Vector3D Force = new Vector3D();
            foreach (CelestrialBody body in CelestrialBodies)
            {
                body.Force = new Vector3D(0, 0, 0); //Kraft jedes Körpers wird resetet.
            }

            //Die folgende Schleife weist jedem Körper die auf ihn wirkende Kraft zu.
            for (int i = 0; i < CelestrialBodies.Count - 1; i++)
            {
                for (int n = i + 1; n < CelestrialBodies.Count; n++)
                {
                    Force = getForce(CelestrialBodies[i], CelestrialBodies[n]); // Berechnet die Kraft zweier Körper
                    CelestrialBodies[i].Force -= Force; //Weist die Kraft entgegengerichtet dem einen Körper zu
                    CelestrialBodies[n].Force += Force; //Weist die Kraft dem anderen Körper zu
                }
            }
        }

        //Gibt die quadrierte Distanz zweier Körper zurück (da sie im nächsten rechenschritt ebenfalls als Quadrat gebraucht wird.
        public double getRadiusSquared(CelestrialBody celestrialBody1, CelestrialBody celestrialBody2)
        {
            double Radius;
            Radius = (Math.Pow(celestrialBody1.Position.X - celestrialBody2.Position.X, 2) + Math.Pow(celestrialBody1.Position.Y - celestrialBody2.Position.Y, 2) + Math.Pow(celestrialBody1.Position.Z - celestrialBody2.Position.Z, 2));
            return Radius;
        }

        //Die Funktion berechnet die Kraft, die zwischen den beiden Körper (1 und 2) wirken
        public Vector3D getForce(CelestrialBody celestrialBody1, CelestrialBody celestrialBody2)
        {
            double doubleForce;
            Vector3D vectorForce;
            Vector3D Einheitsvector;
            double RadiusSquared = getRadiusSquared(celestrialBody1, celestrialBody2);
            double Radius = Math.Sqrt(RadiusSquared);
            doubleForce = celestrialBody1.Mass * celestrialBody2.Mass / RadiusSquared * 6.67f * Math.Pow(10, -11); //Berechnet die Kraft
            Einheitsvector = new Vector3D((celestrialBody1.Position.X - celestrialBody2.Position.X) / Radius, (celestrialBody1.Position.Y - celestrialBody2.Position.Y) / Radius, (celestrialBody1.Position.Z - celestrialBody2.Position.Z) / Radius); //Zeigt die Richtung der Kraft (mit länge 1)
            vectorForce = new Vector3D((Einheitsvector.X * doubleForce), (Einheitsvector.Y * doubleForce), (Einheitsvector.Z * doubleForce));
            return vectorForce;
        }
        #endregion

        //Die Region "Save/Open" regelt das Speichern und Öffnen eines Systems.
        #region Save/Open

        //Speichert das System, also die Liste, als XML-Datei am übergebenen Ort (filename)
        public void Save(string filename)
        {
            using (Stream stream = System.IO.File.Open(filename, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<CelestrialBody>));
                serializer.Serialize(stream, CelestrialBodies);
            }
        }

        //Öffnet eine Xml-Datei, beim Dateipfad (filename), und serialisiert daraus eine Liste
        public List<CelestrialBody> LoadTasks(string filename)
        {
            List<CelestrialBody> Bodies = new List<CelestrialBody>();
            if (System.IO.File.Exists(filename))
            {
                using (Stream stream = System.IO.File.OpenRead(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<CelestrialBody>));
                    Bodies = (List<CelestrialBody>)serializer.Deserialize(stream);

                }
            }
            return Bodies;
        }
        #endregion

        //Die region "Δt-Recommendation" erstellt einen String, der ein Δt für das jeweilige System empfehlt.
        #region Δt-Recommendation

        double[] DistancesToBiggestMassBody;
        public string ΔtRecommendation()
        {
            string ΔtRecomendationString= null; //string wird zurückgesetzt

            DistancesToBiggestMassBody = new double[CelestrialBodies.Count]; //Array wird definiert (grösse durch Anzahl Körper gegeben)

            //Der Körper mit der grössten Masse wird bestimmt.           
            BiggestMassBody = FindBiggestMassBody();

            //Distanzen der Planenten zum Planeten mit der grössten Masse werden bestimmt.
            DistancesToBiggestMassBody = CalcDistanceToBiggestMassBody();

            //Monde werden gesucht (praktisch Gleiche Radien zur grössten Masse ==> Radius des Planet/Mond Systems wird berechnet. Radius wird dem Mond zugewiesen (im Array))
            CheckForPotentialMoons();

            double[] OrbitalPeriods = new double[DistancesToBiggestMassBody.Length]; //Umlaufzeiten werden angenähert berechnet
            for (int i = 0; i < OrbitalPeriods.Length; i++)
            {
                //Für den Mond
                if (MoonFound && i == MoonIndex)
                {
                    OrbitalPeriods[i] = Math.Sqrt(4 * Math.Pow(Math.PI, 2) * Math.Pow(DistancesToBiggestMassBody[i], 3) / (6.67f * Math.Pow(10, -11) * (CelestrialBodies[PlanetMoonIndex].Mass + CelestrialBodies[i].Mass)));

                }

                //Für alle anderen Körper
                else
                {
                    OrbitalPeriods[i] = Math.Sqrt(4 * Math.Pow(Math.PI, 2) * Math.Pow(DistancesToBiggestMassBody[i], 3) / (6.67f * Math.Pow(10, -11) * (BiggestMassBody.Mass + CelestrialBodies[i].Mass)));
                }
            }

            //Diese Schleife erstellt den String
            for (int i = 0 ; i < 4; i++)
            {
                //Die beiden Δt Werte, die den Bereich bestimmen, werden berechnet (für verschiedene Fehlerbereiche)
                double SmalestGoodΔt = (int)(getBiggestOrbitalTime(OrbitalPeriods) / Math.Pow(10, 8-i));
                double biggestGoodΔt = (int)(getsmalestOrbitalTime(OrbitalPeriods) / Math.Pow(10,2+i));
                if (SmalestGoodΔt <= biggestGoodΔt)
                {
                    ΔtRecomendationString = ΔtRecomendationString + "Fehlerbereich: 10^-" +(6+3*i)+ ": Δt zwischen " + (SmalestGoodΔt).ToString() + " und " + (biggestGoodΔt).ToString() + Environment.NewLine;
                }
                else
                {
                    ΔtRecomendationString = ΔtRecomendationString + "Fehlerbereich: 10^-" + (6 + 3 * i) + ": nicht für alle Körper möglich. Die Werte wären: " + (SmalestGoodΔt).ToString() + " und " + (biggestGoodΔt).ToString() + Environment.NewLine;
                }

            }
            return ΔtRecomendationString;
        }


        private CelestrialBody BiggestMassBody;

        //sucht den Körper mit der grössten Masse und gibt diesen zurück
        private CelestrialBody FindBiggestMassBody()
        {
            CelestrialBody BiggestMass = CelestrialBodies[0];
            foreach (CelestrialBody body in CelestrialBodies)
            {
                if (body.Mass > BiggestMass.Mass)
                {
                    BiggestMass = body;
                }
            }
            return BiggestMass;
        }

        //Berechnet die Distanz aller Körper zum schwersten Körper und gibt sie als Array zurück
        private double[] CalcDistanceToBiggestMassBody()
        {
            for (int i = 0; i < CelestrialBodies.Count; i++)
            {
                double Distance = Math.Sqrt(getRadiusSquared(BiggestMassBody, CelestrialBodies[i]));

                if ((BiggestMassBody.Mass / Distance) < Math.Pow(10, 16)) //sehr weit aussen liegende Körper werden nicht beachtet
                {
                    Distance = 0;
                }
                DistancesToBiggestMassBody[i] = Distance;
            }
            return DistancesToBiggestMassBody;
        }

        //Prüft ob Monde vorhanden sind.
        bool MoonFound = false;
        int MoonIndex;
        int PlanetMoonIndex;
        private void CheckForPotentialMoons()
        {
            MoonFound = false;
            double SmalestMoonDistance = 10000000000000000000;
            for (int i = 0; i < DistancesToBiggestMassBody.Length - 1; i++)
            {
                if (DistancesToBiggestMassBody[i] != 0)
                {
                    for (int n = i + 1; n < DistancesToBiggestMassBody.Length; n++)
                    {
                        if (DistancesToBiggestMassBody[n] != 0)
                        {
                            double DistanceRatio = DistancesToBiggestMassBody[i] / DistancesToBiggestMassBody[n];
                            double MoonDistance = Math.Sqrt(getRadiusSquared(CelestrialBodies[i], CelestrialBodies[n]));
                            if (DistanceRatio < 1.01 && DistanceRatio > 0.99 && MoonDistance < (DistancesToBiggestMassBody[i] / 100))
                            {
                                MoonFound = true;
                                if (MoonDistance < SmalestMoonDistance)
                                {
                                    SmalestMoonDistance = MoonDistance;
                                    if (CelestrialBodies[i].Mass < CelestrialBodies[n].Mass)
                                    {
                                        MoonIndex = i; //And diesem Index des Arrays befindet sich der Mond mit der kleinsten Distanz zu seinem Zentralkörper
                                        PlanetMoonIndex = n;
                                    }
                                    else
                                    {
                                        MoonIndex = n;
                                        PlanetMoonIndex = i;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (MoonFound)
            {
                DistancesToBiggestMassBody[MoonIndex] = SmalestMoonDistance;
            }

        }


        //gibt die grösste Umlaufzeit zurück
        private double getsmalestOrbitalTime(double[] OrbitalTime)
        {
            double SmalestTime = 0;
            for (int i = 0; i < OrbitalTime.Length; i++)
            {
                if (OrbitalTime[i] != 0)
                {
                    SmalestTime = OrbitalTime[i];
                    i = OrbitalTime.Length;
                }
            }

            for (int i = 0; i < OrbitalTime.Length; i++)
            {
                if (OrbitalTime[i] != 0 && OrbitalTime[i] < SmalestTime)
                {
                    SmalestTime = OrbitalTime[i];
                }
            }
            return SmalestTime;
        }

        //gibt die grösste Umlaufzeit zurück
        private double getBiggestOrbitalTime(double[] OrbitalTime)
        {
            double biggestTime = 0;
            for (int i = 0; i < OrbitalTime.Length; i++)
            {
                if (OrbitalTime[0] != 0)
                {
                    biggestTime = OrbitalTime[0];
                    i = OrbitalTime.Length;
                }
            }

            for (int i = 0; i < OrbitalTime.Length; i++)
            {
                if (OrbitalTime[i] != 0 && OrbitalTime[i] > biggestTime)
                {
                    biggestTime = OrbitalTime[i];
                }
            }
            return biggestTime;
        }
        #endregion
    }
}
