using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties;


namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen
{
    /// <summary>
    /// Der folgende Code ist die Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        //Wird beim erstellen eines SettingsWindow's ausgeführt.
        public SettingsWindow()
        {
            InitializeComponent();
            main = App.Current.MainWindow as MainWindow;
            txt_Positionstream_NumberOfPoints.Text = Settings.Default.StreamNumberOfLastPositions.ToString();
            txt_Positionstream_Savingfrequency.Text = Settings.Default.OnlySaveEveryNthPosition.ToString();
            Slider_RadiusMultiplicator.Value = Settings.Default.RadiusMultiplicator;
            Slider_RadiusSummand.Value = Settings.Default.RadiusSummand;
            foreach (CelestrialBody body in main.world.CelestrialBodies)
            {
                cbb_TestingBody.Items.Add(body.Name);
                cbb_TestingBody_Copy.Items.Add(body.Name);
            }
            cbb_TestingBody_Copy.SelectedIndex = 0;
        }

        MainWindow main; //ermöglicht das ansprechen der MainWindow Variabeln und Funktionen

        //Das folgende Event wird beim durchführen eines Testlaufes ausgelöst.
        private void btn_Testdurchlauf_Click(object sender, RoutedEventArgs e)
        {
            if(main.world.CelestrialBodies.Count != 0)
            {
                main.TestingIsActive = true; //Testen wird aktiviert (bei Rendering_Event)

                //Alle eingegebenen Werte werden den Variablen zugeteilt
                try
                {
                    Settings.Default.TestingToolDeltaTBase = float.Parse(txt_DeltaTBasis.Text);
                    Settings.Default.TestingToolDeltaTMultiplicator = int.Parse(txt_DeltaTMultiplicator.Text);
                    Settings.Default.NumberOfRevolutionsPerTest = int.Parse(txt_NumberOfRevolutions.Text);
                    if (cbb_TestingBody.SelectedIndex >= 0)
                    {
                        main.BodyForMeasuring = cbb_TestingBody.SelectedIndex;
                    }
                    if (cbb_TestingBody_Copy.SelectedIndex >= 0)
                    {
                        main.CentralBodyIndex = cbb_TestingBody_Copy.SelectedIndex;
                    }
                    main.NumberOfRevolutions = Settings.Default.NumberOfRevolutionsPerTest;
                    main.TestingTool();
                }
                catch (Exception)
                {
                    MessageBox.Show("Bitte nur ganze Zahlen eingeben", "Achtung");
                    return;
                }
            }            
        }

        
        private void btn_ResetTest_Click(object sender, RoutedEventArgs e)
        {
            main.PowerΔt = 0;
            main.MeasureData = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            main.TestingIsActive = false;
            main.settingsWindow = null;
        }

        //Die Einstellungen (settings) werden gespeichert. Dabei werden insbesondere die Daten der Umlaufbahn-Linie gespeichert.
        private void btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if(Settings.Default.StreamNumberOfLastPositions != int.Parse(txt_Positionstream_NumberOfPoints.Text))
                {
                    Settings.Default.StreamNumberOfLastPositions = int.Parse(txt_Positionstream_NumberOfPoints.Text);
                    MessageBox.Show("Die Anzahl der Gespeicherten Punkte wird beim Programmneustart verändert.", "Info");
                }
                Settings.Default.OnlySaveEveryNthPosition = int.Parse(txt_Positionstream_Savingfrequency.Text);
                main.OnlySaveEveryNthPosition = Settings.Default.OnlySaveEveryNthPosition;
            }
            catch(Exception)
            {
                MessageBox.Show("Bitte nur ganze Zahlen eingeben");
            }
            Settings.Default.TestingIsAktive = false;
            Settings.Default.Save();
            MessageBox.Show("Die Daten wurden gespeichert");
        }

        //Verändert den RadiusMultiplicator, je nach Wert des Sliders
        private void Slider_RadiusMultiplicator_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loaded)
            {
                try
                {
                    Settings.Default.RadiusMultiplicator = (int)(Slider_RadiusMultiplicator.Value);
                    main.world.RadiusMultiplicator = Settings.Default.RadiusMultiplicator;
                }
                catch (Exception)
                {

                }
            }
        }

        //Verändert den RadiusSummand, je nach Wert des Sliders
        private void Slider_RadiusSummand_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {            
            if (loaded)
            {
                try
                {
                    Settings.Default.RadiusSummand = (int)(Slider_RadiusSummand.Value);
                    main.world.RadiusSummand = Settings.Default.RadiusSummand;
                }                
                catch (Exception)
                {

                }
            }
        }

        
        bool loaded = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
        }
        
        //Aktiviert oder deaktiviert die Verfolgungslinie
        private void btn_LinieDeaktivieren_Click(object sender, RoutedEventArgs e)
        {
            main.activpStream = !main.activpStream;
            if(main.activpStream)
            {
                btn_LinieDeaktivieren.Content = "deaktivieren";
            }
            else
            {
                btn_LinieDeaktivieren.Content = "aktivieren";
            }
        }
    }
}
