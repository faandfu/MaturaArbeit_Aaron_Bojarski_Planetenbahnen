using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties;
using Microsoft.Win32;

/*
 * Sehr geehrter Herr Gross
 * Sehr geehrter Herr Veselcic
 * 
 * Folgend finden Sie den Quellcode meines Programms. 
 * Kommentare / Erklärungen finden Sie meistens überhalb der jeweiligen Funktion.
 * Ich hoffe Sie finden sich zurecht.
 * 
 * Freundliche Grüsse
 * Aaron Bojarski
 */
namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Wird das Programm gestartet, so wird dieser Code als erstes ausgeführt
        #region StartProgramm
        public MainWindow()
        {
            InitializeComponent();
        }


        public int height, width;
        WriteableBitmap writeableBmp; //Darauf wird gezeichnet
        public GameWorld world;
        string filename = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "world.xml";

        private void ViewPort_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            width = (int)this.ViewPortContainer.ActualWidth;
            height = (int)this.ViewPortContainer.ActualHeight;
            writeableBmp = BitmapFactory.New(width - 110, height);
            ViewPort.Source = writeableBmp;
            CreateWorld(); 
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            CenteredBodyZero.Position = new Vector3D(0, 0, 0);            
        }
        #endregion

        #region One Tick
        float TimePassed; //die Vergangene Zeit in Erd-Tagen
        int CalculationsPerFrame = 10;
        public bool TestingIsActive = Settings.Default.TestingIsAktive;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            writeableBmp.Clear(); //Untergrund wird gecleared
            BitmapSettings(); //Der Hintergrund wird schwarz und die Verfolgungslinie / Umlaufbahn-Punkte werden gezeichnet
            if (world.CelestrialBodies.Count > 0)
            {
                if (AnimationRunning) //Ist True, wenn Benutzer auf START drückt
                {
                    for (int i = 0; i < CalculationsPerFrame; i++) //Führt Berechnung je nach eingestellter Animationsgeschwindigkeit unterschiedlich oft durch
                    {
                        world.GameTick(Δt); //Berechnung wird mit Δt einmal durchgeführt
                        if (TestingIsActive) //Wenn der Benutzer einen Test durchführt, dann ist TestingIsActive = true
                        {
                            if (StopIfFullCircle()) //Stoppt die Berechnung nach vollendung einer Umdrehung
                            {
                                i = CalculationsPerFrame;
                            }
                        }
                    }

                    TimePassed = (float)((world.CalculatedTime) / 3600 / 24); 
                    txt_TimePassed.Text = TimePassed.ToString() + " d"; //gibt die verstrichene Zeit aus
                    ShowSelectedBodyInformation(); //zeigt die Daten des gewählten Körpers

                }
                Check_If_MaxCalculations_Can_Be_Achieved(); //Checkt ob genug Rechenleistung verfügbar ist
                world.DrawWorld(writeableBmp, width, height, ZoomFaktor); //Zeichnet alle Körper der "Welt"
            }
        }

        //Die folgende Funktion ist dafür verantwortlich, dass, während des Testens, die Berechnung nach einer Umdrehung gestoppt wird.
        bool HalfDone = false; //Halbe Umdrehung vollendet
        int RevolutionsMadeCount; //zählt die Anzahl der Umdrehungen
        public int NumberOfRevolutions = Settings.Default.NumberOfRevolutionsPerTest;
        public int CentralBodyIndex = 0;
        private bool StopIfFullCircle()
        {
            if (world.CelestrialBodies.Count != 0)
            {
                if (HalfDone == false)
                {
                    if (world.CelestrialBodies[BodyForMeasuring].Position.Y < world.CelestrialBodies[CentralBodyIndex].Position.Y)
                    {
                        HalfDone = true;
                    }
                }
                if (HalfDone == true)
                {
                    if (world.CelestrialBodies[BodyForMeasuring].Position.Y > world.CelestrialBodies[CentralBodyIndex].Position.Y)
                    {
                        RevolutionsMadeCount++;
                        HalfDone = false;
                    }
                }
                if (RevolutionsMadeCount > NumberOfRevolutions - 1)
                {
                    AnimationRunning = false;
                    if (BodyForMeasuring < world.CelestrialBodies.Count)
                    {
                        OutputMeasuredDataForOneTest(); //Gibt die gemessenen Daten aus
                    }
                    RevolutionsMadeCount = 0;
                    return true; //gibt true zurück, sobald die gewünschte Anzahl Umdrehungen vollendet wurde
                }
            }
            return false;
        }


        //Diese Funktion gibt dem Benutzer die Körper-Daten des ausgewählten Körpers aus
        private void ShowSelectedBodyInformation()
        {
            CelestrialBody SelectedBody;
            if (world.CelestrialBodies != null)
            {
                if (cbbSelectedBody.SelectedIndex > -1)
                {
                    SelectedBody = world.CelestrialBodies[cbbSelectedBody.SelectedIndex];
                    txtName_Selected.Text = SelectedBody.Name;
                    txtPositionX_Selected.Text = SelectedBody.Position.ToString();
                    txtVelocity_Selected.Text = SelectedBody.Velocity.Length.ToString(); // (Math.Sqrt(Math.Pow(SelectedBody.Velocity.X, 2) + Math.Pow(SelectedBody.Velocity.Y, 2) + Math.Pow(SelectedBody.Velocity.Z, 2))).ToString();
                    txtForce_Selected.Text = SelectedBody.Force.Length.ToString();//Math.Sqrt(Math.Pow(SelectedBody.Force.X, 2) + Math.Pow(SelectedBody.Force.Y, 2) + Math.Pow(SelectedBody.Force.Z, 2)).ToString();
                }
            }
        }

        //falls das Rendering-Event mehr als 16.7ms benötigt, wird dem Benutzer mit dieser Funktion ausgegeben, dass zu wenig Rechenleistung verfügbar ist
        int MorethenOnceUnder60Frames;
        bool colorRed;
        private void Check_If_MaxCalculations_Can_Be_Achieved()
        {
            //Zeit pro Bild analysieren
            if (world.ElapsedMillisecondsForOneTick > 16.7) 
            {
                if (MorethenOnceUnder60Frames > 15) //Um ständiges Flackern zu vermeiden wurde diese Bedingung eingeführt
                {
                    if (!colorRed)
                    {
                        lbl_CalcPerFrame.Background = new SolidColorBrush(Colors.Red);
                        colorRed = true;
                    }
                }
                MorethenOnceUnder60Frames++;
            }
            else
            {
                if (colorRed)
                {
                    lbl_CalcPerFrame.Background = new SolidColorBrush(Colors.Transparent);
                    colorRed = false;
                }
                MorethenOnceUnder60Frames = 0;
            }
        }
        #endregion

        //Die Region "Create World" erstellt eine "GameWorld"-Klasse. In ihr werden ebenfalls Körper, die im Quellcode stehen, der welt hinzugefügt.
        #region Create World
        private void CreateWorld()
        {
            world = new GameWorld();

            //Hier werden die Körper definiert und hinzugefügt
            #region Celestrialbodies Added in Code                       

            var Sonne = new CelestrialBody()
            {
                Name = "Sonne",
                Radius = 695508000, 
                Mass = (float)(1989 * Math.Pow(10, 27)),
                Position = new Vector3D(0, 0, 0),
                Velocity = new Vector3D(0, 0, 0),  //14.4
                BodyColor = Color.FromRgb(255, 204, 0) //255, 255, 0
            };


            var Erde = new CelestrialBody()
            {
                Name = "Erde",
                Radius = 3397000, 
                Mass = (float)(597 * Math.Pow(10, 0)), //22
                Position = new Vector3D(-149579000000, 0, 0), //149579000000  152097703653.436
                Velocity = new Vector3D(0, 29781, 0), //29781  //29293.3398
                BodyColor = Color.FromRgb(0, 102, 255) //0, 178, 238
            };            

            
            var DoppelStern = new CelestrialBody()
            {
                Name = "Doppelstern",
                Radius = 695508000,
                Mass = (float)(1949 * Math.Pow(10, 27)),
                Position = new Vector3D(149984000000f*2.5, 0, 0),
                Velocity = new Vector3D(0, -15781, 0),
                BodyColor = Color.FromRgb(185, 155, 0)
            };
            
            var Mond = new CelestrialBody()
            {
                Name = "Mond",
                Radius = 1737000,
                Mass = (float)(7349 * Math.Pow(10, 18)),
                Position = new Vector3D(-152098320000 - 384400000, 0, 0),
                Velocity = new Vector3D(0, 29293.43+1000, 0),
                BodyColor = Color.FromRgb(166, 166, 166)                
            };            
            
            
            var Merkur = new CelestrialBody()
            {
                Name = "Merkur",
                Radius = 2439764,
                Mass = (float)(3.301 * Math.Pow(10, 23)),
                Position = new Vector3D(-69817078612.5807, 0, 0),
                Velocity = new Vector3D(0, 38861.52575, 0),
                BodyColor = Color.FromRgb(205, 186, 150)
            };


            var Venus = new CelestrialBody()
            {
                Name = "Venus",
                Radius = 6051590,
                Mass = (float)(4.869 * Math.Pow(10, 24)), //24
                Position = new Vector3D(-108941853970.944, 0, 0), //-108941853970.944
                Velocity = new Vector3D(0, 34786.9444, 0), //34786.9444, Kreis = 34896.6
                BodyColor = Color.FromRgb(131, 139, 131) //210, 180, 140
            };


            var Mars = new CelestrialBody()
            {
                Name = "Mars",
                Radius = 3397000,
                Mass = (float)(6.419 * Math.Pow(10, 23)), //23
                Position = new Vector3D(-249228732634.771, 0, 0),
                Velocity = new Vector3D(0, 21973.3010, 0),
                BodyColor = Color.FromRgb(255, 64, 64)
            };

            
            var Jupiter = new CelestrialBody()
            {
                Name = "Jupiter",
                Radius = 71492680,
                Mass = (float)(1.899 * Math.Pow(10, 27)), //27
                Position = new Vector3D(816081448223.773, 0 , 0), //-778360000000, 816081448223.773
                Velocity = new Vector3D(0, -12440.8941, 0), //13055, -12440.8941
                BodyColor = Color.FromRgb(255, 99, 71)
            };


            var Saturn = new CelestrialBody()
            {
                Name = "Saturn",
                Radius = 60267140,
                Mass = (float)(5.685 * Math.Pow(10, 26)),
                Position = new Vector3D(1503983436445.240, 0, 0),
                Velocity = new Vector3D(0, -9136.4804, 0),
                BodyColor = Color.FromRgb(255, 218, 185)
            };


            var Uranus = new CelestrialBody()
            {
                Name = "Uranus",
                Radius = 25559000,
                Mass = (float)(8.683 * Math.Pow(10, 25)),
                Position = new Vector3D(-3006389384147.660, 0, 0),
                Velocity = new Vector3D(0, 6485.9721, 0),
                BodyColor = Color.FromRgb(187, 255, 255)
            };


            var Neptun = new CelestrialBody()
            {
                Name = "Neptun",
                Radius = 24764000,
                Mass = (float)(1.0243 * Math.Pow(10, 26)),
                Position = new Vector3D(-4536874314626.520, 0, 0),
                Velocity = new Vector3D(0, 5385.6563, 0),
                BodyColor = Color.FromRgb(65, 105, 225)
            };

            var Pluto = new CelestrialBody()
            {
                Name = "Pluto",
                Radius = 1188000,
                Mass = (float)(1.303 * Math.Pow(10, 22)),
                Position = new Vector3D(7375912320000.00, 0, 0),
                Velocity = new Vector3D(0, -3676.71, 0),
                BodyColor = Color.FromRgb(166, 166, 166)
            };


            var Komet = new CelestrialBody()
            {
                Name = "Komet",
                Radius = 3000000,
                Mass = (float)Math.Pow(10, 2),
                Position = new Vector3D(14*149600000000f, 7*-149600000000f, 0),
                Velocity = new Vector3D(-25000, 10000, 0),
                BodyColor = Color.FromRgb(235, 150, 255)
            };

            
            
            world.AddCelestrialBody(Sonne);
            //world.AddCelestrialBody(DoppelStern);
            //world.AddCelestrialBody(Merkur);
            //world.AddCelestrialBody(Venus);
            world.AddCelestrialBody(Erde);            
            //world.AddCelestrialBody(Mond);
            //world.AddCelestrialBody(Mars);              
            //world.AddCelestrialBody(Jupiter);             
            //world.AddCelestrialBody(Saturn);            
            //world.AddCelestrialBody(Uranus);
            //world.AddCelestrialBody(Neptun);
            //world.AddCelestrialBody(Pluto);
            //world.AddCelestrialBody(Komet);
            #endregion
        }
        #endregion

        //Die Region "UserInteractionEvents" enthält alle Events, die durch Knöpfe, Slider, etc. hervorgerufen werden.
        //Die Label, Knöpfe etc. sind dabei in ihrer Benennung ersichtlich
        //lbl = Label
        //btn = Button
        //cbb = ComboBox
        //Slider = Slider
        #region UserInteractionEvents(btn/txt/slider)    
                       
        //Diese Funktion löscht alles, bzw. resetet alle Werte.
        private void clearAll()
        {
            world.CelestrialBodies.Clear();
            cbbSelectedBody.Items.Clear();
            cbbSelectedBody.SelectedIndex = -1;
            world.CalculatedTime = 0;
            cbbCenteredBody.Items.Clear();
            cbbCenteredBody.SelectedIndex = -1;
            ToggleCenteredBody = false;
            world.Centeredbody = CenteredBodyZero;
            cbbCenteredBody.IsEnabled = false;
        }


        //Diese Funktion verändert die Anzahl der Berechnungen pro Bild, je nach dem, an welcher Position sich der Slider befindet.
        //Zusätzlich gibt es diesen Wert dem Benutzer aus.
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CalculationsPerFrame = (int)Math.Pow(10, e.NewValue); //e.NewValue ist der Wert, an dem der Slider ist.
            try
            {
                lbl_CalcPerFrame.Content = "Berechnungen pro" + Environment.NewLine +"Frame = " + CalculationsPerFrame.ToString();

            }
            catch (Exception)
            {

            }
        }

        //Dieses Event wird durch den Knopf Stop/Start ausgelöst. Es stopt und startet die Berechnung.
        public bool AnimationRunning = false;
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (AnimationRunning == true)
            {
                AnimationRunning = false;
                btnStop.Content = "Start";
            }
            else
            {
                AnimationRunning = true;
                btnStop.Content = "Stop";
            }
        }

        //Dieses Event fragt den ΔtRecommendation-String der GameWorld Klasse ab und gibt ihn dem Benutzer aus
        private void btn_deltaT_recom_Click(object sender, RoutedEventArgs e)
        {
            if (world.CelestrialBodies.Count > 0)
            {
                MessageBox.Show(world.ΔtRecommendation(), "Δt Vorschlag");
            }
        }

        //Aktiviert und deaktiviert, dass ein Körper im Zentrum fixiert ist.
        bool ToggleCenteredBody = false;
        CelestrialBody CenteredBodyZero = new CelestrialBody(); //Falls das Zentrieren deaktiviert ist, so ist dieser Körper mit der Position (0,0,0), der Körper, der beim Zeichnen übergeben wird
        private void btnToggleCentered_Click(object sender, RoutedEventArgs e)
        {
            ToggleCenteredBody = !ToggleCenteredBody;
            cbbCenteredBody.IsEnabled = !cbbCenteredBody.IsEnabled;
            if(ToggleCenteredBody)
            {
                btn_ToggleCenter.Content = "deaktivieren";
            }
            if (!ToggleCenteredBody)
            {
                world.Centeredbody = CenteredBodyZero;
                btn_ToggleCenter.Content = "aktivieren";
            }
            else if (cbbCenteredBody.SelectedIndex > -1)
            {
                world.Centeredbody = world.CelestrialBodies[cbbCenteredBody.SelectedIndex];
            }

            //Updatet die Umlaufbahn-Linie            

            for (int i = 0; i < CountOfLastPositions; i++)
            {
                DrawPositionsStream[i] = new Vector3D((((width / 2f) - ((width / 2f - (PositionSelectedStreamBody[i].X)) * ZoomFaktor))), (((height / 2f) - ((height / 2f - (PositionSelectedStreamBody[i].Y)) * ZoomFaktor))), 0);
            }            
        }

        //schliesst alle offenen Fenster
        private void window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        //Wird der ausgewählte Körper gewechselt, so werden seine Informationen ausgegeben
        private void cbbSelectedBody_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedBodyInformation();
        }

        //Wird der ausgewählte Körper des Zentrierens gewechselt, so wird dieser in der GameWorld gewechselt.
        private void cbbCenteredBody_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (world.CelestrialBodies.Count > cbbCenteredBody.SelectedIndex && cbbCenteredBody.SelectedIndex > -1)
            {
                world.Centeredbody = world.CelestrialBodies[cbbCenteredBody.SelectedIndex];
            }
        }

        //Dieses Event wird ausgelöst, sobald die Enter-Taste im Δt Textfeld betätigt wird.
        //Dann wird das Δt auf den eingegebenen Wert gesetzt
        float Δt = 500;
        private void txt_deltaT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Δt = float.Parse(txt_deltaT.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Bitte nur Zahlen eingeben");
                }
            }
        }
        
        #endregion

        //Die nächste Region enthält alle Events, die durch interaktion mit der Visualisierung hervorgerufen werden.
        #region SurfaceInteraction (zoom/move planets)   

        public int mousemoveX;
        public int mousemoveY;
        int mouseStartX;
        int mouseStartY;
        int mouseEndX;
        int mouseEndY;
        //Diese Funktion speichert die Bildkoordinate der Cursors beim Drücken der Maustaste.
        private void ViewPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseStartX = (int)e.GetPosition(ViewPortContainer).X;
            mouseStartY = (int)e.GetPosition(ViewPortContainer).Y;
        }

        //Dieses Event wird beim Loslassen der Maustaste ausgelöst.
        private void ViewPort_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!ToggleCenteredBody || cbbCenteredBody.SelectedIndex < 0)
            {
                //Als erstes wird berechnet, um wie weit verschoben werden soll.
                mouseEndX = (int)e.GetPosition(ViewPortContainer).X;
                mouseEndY = (int)e.GetPosition(ViewPortContainer).Y;
                mousemoveX = mouseEndX - mouseStartX;
                mousemoveY = mouseEndY - mouseStartY;
                //Jeder Körper wird verschoben
                foreach (CelestrialBody body in world.CelestrialBodies)
                {
                    body.Position = new Vector3D(body.Position.X + mousemoveX / ZoomFaktor, body.Position.Y + mousemoveY / ZoomFaktor, body.Position.Z);
                }
                //Jeder Punkt der Umlaufbahn-Linie wird verschoben
                for (int i = 0; i < PositionSelectedStreamBody.Length; i++)
                {
                    PositionSelectedStreamBody[i] = new Vector3D(PositionSelectedStreamBody[i].X + mousemoveX / ZoomFaktor, PositionSelectedStreamBody[i].Y + mousemoveY / ZoomFaktor, PositionSelectedStreamBody[i].Z);
                    DrawPositionsStream[i] = new Vector3D(DrawPositionsStream[i].X + mousemoveX, DrawPositionsStream[i].Y + mousemoveY, DrawPositionsStream[i].Z);
                }
            }
        }


        float DeltaZoomFaktorPerMouseWheel = (5f / 4f);
        float ZoomFaktor = Settings.Default.ZoomRate;
        //Das folgende Event wird beim Drehen des Mausrades ausgelöst. Dabei wird der Zoomfaktor verändert.
        private void ViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int ZoomRate = e.Delta;
            if (ZoomRate < 0)
            {
                ZoomFaktor = ZoomFaktor / DeltaZoomFaktorPerMouseWheel;
            }

            if (ZoomRate > 0)
            {
                ZoomFaktor = ZoomFaktor * DeltaZoomFaktorPerMouseWheel;
            }

            //Die folgenden Zeilen passen die Umlaufbahn-Linie an. Sie wird mit dem Neuen Zoomfaktor neu berechnet.
            if (ToggleCenteredBody && cbbCenteredBody.SelectedIndex > -1)
            {
                for (int i = 0; i < CountOfLastPositions; i++)
                {
                    DrawPositionsStream[i] = new Vector3D((((width / 2f) - ((width / 2f - (PositionSelectedStreamBody[i].X - PositionCenteredBody[i].X)) * ZoomFaktor))), (((height / 2f) - ((height / 2f - (PositionSelectedStreamBody[i].Y - PositionCenteredBody[i].Y)) * ZoomFaktor))), 0);
                }
            }
            else
            {
                for (int i = 0; i < CountOfLastPositions; i++)
                {
                    DrawPositionsStream[i] = new Vector3D((((width / 2f) - ((width / 2f - (PositionSelectedStreamBody[i].X)) * ZoomFaktor))), (((height / 2f) - ((height / 2f - (PositionSelectedStreamBody[i].Y)) * ZoomFaktor))), 0);
                }
            }
        }

        #endregion

        //Die Region "MnuStripEvents" enthält alle Events die durch das Klicken auf die Menu-Leiste hervorgerufen werden.
        #region MnuStripEvents
        //Das folgende Event öffnet eine XML-Datei beim eingegebenen Speicherort
        private void MnuOpen_Click(object sender, RoutedEventArgs e)
        {
            FileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                clearAll();
                try
                {
                    world.CelestrialBodies = world.LoadTasks(filename);
                }
                catch (Exception)
                {
                    MessageBox.Show("Datei konnte nicht geöffnet werden", "Fehler");
                }
                UpdateComboBox();
            }
        }

        //Die beiden ComboBoxen werden mit den neuen Körpern aktualisiert.
        public void UpdateComboBox()
        {
            cbbSelectedBody.Items.Clear();
            cbbCenteredBody.Items.Clear();
            foreach (CelestrialBody body in world.CelestrialBodies)
            {
                cbbSelectedBody.Items.Add(body.Name);
                cbbCenteredBody.Items.Add(body.Name);
            }
        }

        //Speichert ein System beim Dateipfad "filename"
        private void MnuSave_Click(object sender, RoutedEventArgs e)
        {
            world.Save(filename);
        }

        //Speichert ein System beim eingegebenen Dateipfad 
        private void MnuSpeichernUnter_Click(object sender, RoutedEventArgs e)
        {
            FileDialog savefiledialog = new SaveFileDialog();
            if (savefiledialog.ShowDialog() == true)
            {
                world.Save(savefiledialog.FileName);
            }

        }
        
        //Beendet die Application
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Sind Sie sicher, dass Sie das Programm schliessen wollen? Nicht gespeicherte Systeme gehen verloren.", "Bestätigung", MessageBoxButton.YesNo))
            {
                App.Current.Shutdown();
            }
        }

        //Löscht das System, falls der Benutzer es bestätigt
        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Sind Sie sicher, dass Sie alle Körper löschen wollen?", "Bestätigung", MessageBoxButton.YesNo))
            {
                clearAll();
            }
        }

        //Öffnet das Einstellungsfenster.
        public SettingsWindow settingsWindow;
        private void MnuSettings_Click(object sender, RoutedEventArgs e)
        {
            if(settingsWindow == null)
            {
                settingsWindow = new SettingsWindow();
            }            
            settingsWindow.Show();
            settingsWindow.Activate();
        }

        //Öffnet das Hilfe-Fenster.
        HelpWindow helpWindow;
        private void MnuHelp_Click(object sender, RoutedEventArgs e)
        {
            helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        //Öffnet den Konfigurations-Manager
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ConfigurationManager configurationManager = new ConfigurationManager();
            configurationManager.Show();
        }
        #endregion


        //Die folgende Region enthält die Funktionen die das Testen und die Ausgabe der Daten handeln.
        #region Testing
        public int PowerΔt;        
        public void TestingTool()
        {
            Settings.Default.TestingIsAktive = true;
            
            Δt = (float)(Settings.Default.TestingToolDeltaTMultiplicator * Math.Pow(Settings.Default.TestingToolDeltaTBase, PowerΔt)); //Δt wird berechnet (aus den eingegebenen Werten des Nutzers)
            txt_deltaT.Text = Δt.ToString();
            clearAll();
            try
            {
                world.CelestrialBodies = world.LoadTasks(filename); //öffnet die Datei neu, um wieder beim Startwert zu sein.
            }
            catch(Exception)
            {
                MessageBox.Show("Datei konnte nicht geöffnet werden", "Fehler");
            }
            UpdateComboBox();
            AnimationRunning = true; //startet die Berechnung
            PowerΔt++;
        }

        //Die folgende Funktion berechnet die Daten, formatiert sie und gibt sie dem Benutzer aus.
        public int BodyForMeasuring =Settings.Default.BodyForMeasuring;
        public string MeasureData;
        private void OutputMeasuredDataForOneTest()
        {
            TimePassed = (float)((world.CalculatedTime) / 3600 / 24);
            float Time = (float)(TimePassed - (world.CelestrialBodies[BodyForMeasuring].Position.Y- world.CelestrialBodies[CentralBodyIndex].Position.Y) / world.CelestrialBodies[BodyForMeasuring].Velocity.Length / 24 / 3600) / Settings.Default.NumberOfRevolutionsPerTest;
            MeasureData = MeasureData + (Δt).ToString() + (char)09 + Settings.Default.NumberOfRevolutionsPerTest.ToString() + (char)09 + (char)09 + (char)09 + (char)09 + Time.ToString() + (char)09 + (world.CelestrialBodies[BodyForMeasuring].Position.X-world.CelestrialBodies[CentralBodyIndex].Position.X).ToString() + (char)09 + (world.CelestrialBodies[BodyForMeasuring].Position.Y- world.CelestrialBodies[CentralBodyIndex].Position.Y).ToString() + Environment.NewLine;
            settingsWindow.txt_TestingResults.Text = (MeasureData);
        }
        #endregion

        //Die folgende Region enthält die Funktion, welche die Umlaufbahn-Linie berechnet und zeichnet.
        #region Stream
        static int CountOfLastPositions = Settings.Default.StreamNumberOfLastPositions;
        Vector3D[] PositionSelectedStreamBody = new Vector3D[CountOfLastPositions]; //Speichert die Position des Körpers
        Vector3D[] PositionCenteredBody = new Vector3D[CountOfLastPositions]; //Speichert die Position des Zentrierten Körpers
        Vector3D[] DrawPositionsStream = new Vector3D[CountOfLastPositions]; //Speichert die Bild-Koordinate
        int currentArrayPosition;
        public bool activpStream = true;
        int OnlySaveEveryNthPositionIndex = 1;
        public int OnlySaveEveryNthPosition = Settings.Default.OnlySaveEveryNthPosition;

        
        private void BitmapSettings()
        {
            writeableBmp.FillRectangle(0, 0, width - 110, height, Color.FromRgb(0, 0, 0)); //Zeichnet den schwarzen Hintergrund
            if (activpStream && cbbSelectedBody.SelectedIndex > -1)
            {
                if (AnimationRunning && world.CelestrialBodies != null) //Nur wenn berechnet wird, wird auch die Umlaufbahn-Linie aktualisiert
                {
                    if (currentArrayPosition < CountOfLastPositions) //handelt die Array-Position/Index
                    {
                        if (OnlySaveEveryNthPositionIndex + 1 > OnlySaveEveryNthPosition) //checkt, ob dieser Punkt gespeichert werden muss. Das ist nicht der Fall, wenn der Benutzer ausgewählt hat, dass z.B. nur jeder zweite Punkt gespeichert werden soll
                        {
                            PositionSelectedStreamBody[currentArrayPosition] = world.CelestrialBodies[cbbSelectedBody.SelectedIndex].Position; //Ruft die Position des Körpers ab
                            //Falls ein Körper zentriert ist, so wird der Bildpunkt folgendermassen berechnet
                            if (ToggleCenteredBody && cbbCenteredBody.SelectedIndex > -1)
                            {
                                PositionCenteredBody[currentArrayPosition] = world.CelestrialBodies[cbbCenteredBody.SelectedIndex].Position;
                                DrawPositionsStream[currentArrayPosition] = new Vector3D((((width / 2f) - ((width / 2f - (PositionSelectedStreamBody[currentArrayPosition].X - PositionCenteredBody[currentArrayPosition].X)) * ZoomFaktor))), (((height / 2f) - ((height / 2f - (PositionSelectedStreamBody[currentArrayPosition].Y - PositionCenteredBody[currentArrayPosition].Y)) * ZoomFaktor))), 0);
                            }
                            //Falls KEIN Körper zentriert ist, so wird der Bildpunkt folgendermassen berechnet
                            else
                            {
                                DrawPositionsStream[currentArrayPosition] = new Vector3D((((width / 2f) - ((width / 2f - (PositionSelectedStreamBody[currentArrayPosition].X)) * ZoomFaktor))), (((height / 2f) - ((height / 2f - (PositionSelectedStreamBody[currentArrayPosition].Y)) * ZoomFaktor))), 0);

                            }
                            currentArrayPosition++;
                            OnlySaveEveryNthPositionIndex = 1;
                        }
                        else
                        {
                            OnlySaveEveryNthPositionIndex++;
                        }
                    }
                    //Falls das Array gefüllt wurde, so wird wieder von Vorne begonnen es zu überschreiben
                    if (currentArrayPosition > CountOfLastPositions - 1)
                    {
                        currentArrayPosition = 0;
                    }
                }
                //Die folgende Schleife zeichnet alle Punkte auf dem Bild
                for (int i = 0; i < CountOfLastPositions; i++)
                {
                    writeableBmp.FillEllipseCentered((int)DrawPositionsStream[i].X, (int)DrawPositionsStream[i].Y, 2, 2, Color.FromRgb(230,230,230));
                }
            }
        }
        #endregion
    }
}
