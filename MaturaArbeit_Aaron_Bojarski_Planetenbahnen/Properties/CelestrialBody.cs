using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Media3D;


namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties
{
    //Die CelestrialBody Klasse definiert einen Himmelskörper. Alle seine Eigenschaften werden in dieser Klasse gespeichert. Auch die Positionsberechnung des einzelnen Körpers erfolgt in dieser Klasse
    public class CelestrialBody
    {
        public Vector3D Position { get; set; }

        public Vector3D Velocity { get; set; }

        public Vector3D Force { get; set; }

        public Vector2 DrawPosition;

        public float Mass { get; set; }

        public float Radius { get; set; }

        public string Name { get; set; }

        public Color BodyColor { get; set; }

        //Die folgende Funktion zeichnet den einzelnen Körper auf den Hintergrund (surface)
        //Auf die Position des Körpers hat die folgende Funktion keinen Einfluss. Nur die Bildkoordinate wird verändert
        public void DrawSingleBody(WriteableBitmap surface, int width, int height, double ZoomFaktor, CelestrialBody CenteredBody, int RadiusMultiplicator, int RadiusSummand)
        {
            //Diese Region regelt die Grössenänderung, falls die Z-Koordinate benutzt wird. 
            //Sie ändert den Zoomfaktor jedes einzelnen Körpers. So kann die Z-Koordinate dargestellt werden.
            #region Z-Komponente (grössenänderung)
            double RadiusZKomponent = 1;
            double RadiusZKomponentCenteredBody = 1;
            if (Position.Z != 0)
            {
                RadiusZKomponent = 1 + (Position.Z * ZoomFaktor/500 ); //regelt die Z-Darstellung der Z-Koordinate des jeweiligen Körpers
                if (RadiusZKomponent < 0)
                {
                    RadiusZKomponent = 0.0000001;
                }
            }
            if (CenteredBody.Position.Z != 0)
            {
                RadiusZKomponentCenteredBody = 1 + (CenteredBody.Position.Z * ZoomFaktor/500 ); //regelt die Z-Darstellung, falls ein Körper in der zentriert wurde.
                if (RadiusZKomponentCenteredBody < 0)
                {
                    RadiusZKomponentCenteredBody = 0.0000001;
                }
            }
            ZoomFaktor = RadiusZKomponentCenteredBody / RadiusZKomponent * ZoomFaktor;
            #endregion

            //Die folgenden 2 Code-Zeilen berechnen den Bildpunkt (X und Y Koordinate) an welchem der Körper gezeichnet werden soll
            //Dabei wird mit dem Zoomfaktor die Eigentliche Position "verkleinert"
            DrawPosition.X = (float)((width / 2f) - ((width / 2f - (Position.X-CenteredBody.Position.X)) * ZoomFaktor ));
            DrawPosition.Y = (float)((height / 2f) - ((height / 2f - (Position.Y - CenteredBody.Position.Y)) * ZoomFaktor));

            //Der Körper wird an den berechneten BildKoordinaten gezeichnet. 
            //Der Radius wird, neben dem Zoomfaktor, ebenfalls durch die Radienveränderung beeinflusst. Der Benutzer steuert es über die Einstellungen. "1" wird addiert, um zu gewährleisten, dass jeder Körper mit mind. 1 Pixelgrösse gezeichnet wird.
            //Die Farbe ist über die Farb-Property des Körpers definiert
            surface.FillEllipseCentered((int)DrawPosition.X, (int)DrawPosition.Y, (int)((Radius + RadiusSummand) * RadiusMultiplicator * ZoomFaktor) + 1, (int)((Radius + RadiusSummand) * RadiusMultiplicator * ZoomFaktor) + 1, BodyColor);
        }


        private Vector3D LastForce { get; set; }

        public void PositionCalculation(double Δt)
        {
            //Die Position wird durch das Leapfrog-Verfahren neu berechnet.
            Position = new Vector3D(Position.X + Velocity.X * Δt + (Force.X / Mass / 2 * Math.Pow(Δt, 2)), Position.Y + Velocity.Y * Δt + (Force.Y / Mass / 2 * Math.Pow(Δt, 2)), Position.Z + Velocity.Z * Δt + (Force.Z / Mass / 2 * Math.Pow(Δt, 2)));
            LastForce = Force; //Die wirkende Kraft, bzw. Beschleinigung, wird für den nächsten Rechenschritt gespeichert.

            //Diese beiden rauskommentierten Zeilen sind die Berechnungen des Euler-Verfahrens. 
            //Velocity = new Vector3D(Velocity.X + (Force.X / Mass) * Δt, Velocity.Y + Force.Y / Mass * Δt, Velocity.Z + Force.Z / Mass * Δt);
            //Position = new Vector3D(Position.X  + Velocity.X * Δt, Position.Y  + Velocity.Y * Δt, Position.Z + Velocity.Z * Δt);
        }

        public void VelocityCalculation(double Δt)
        {
            //Die Geschwindigkeit wird berechnet. Die Formel ist durch das Leapfrog-Verfahren definiert.
            Velocity = new Vector3D(Velocity.X + ((Force.X+LastForce.X) / Mass / 2) * Δt, Velocity.Y + ((Force.Y + LastForce.Y) / Mass / 2) * Δt, Velocity.Z + ((Force.Z + LastForce.Z) / Mass / 2) * Δt);

        }
    }
}
