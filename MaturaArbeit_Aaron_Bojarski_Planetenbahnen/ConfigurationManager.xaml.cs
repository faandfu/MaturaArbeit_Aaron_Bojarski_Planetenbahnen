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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using MaturaArbeit_Aaron_Bojarski_Planetenbahnen.Properties;


namespace MaturaArbeit_Aaron_Bojarski_Planetenbahnen
{
    /// <summary>
    /// Interaktionslogik für RecordingWindow.xaml
    /// </summary>
    public partial class ConfigurationManager : Window
    {
        MainWindow main;
        public ConfigurationManager()
        {
            InitializeComponent();
            main = App.Current.MainWindow as MainWindow;
            main.AnimationRunning = false;
            main.btnStop.Content = "Start";
            AddBodiesToComboBox();
        }

        //Updatet die Comboboxen, wenn ein Körper gelöscht oder hinzugefügt wird
        private void AddBodiesToComboBox()
        {
            cbb_bodyToChange.Items.Clear();
            foreach (CelestrialBody body in main.world.CelestrialBodies)
            {
                cbb_bodyToChange.Items.Add(body.Name);                
            }
            main.UpdateComboBox();
        }

        //Das Event löscht den ausgewählten Körper
        private void btn_DeleteBody_Click(object sender, RoutedEventArgs e)
        {
            if(cbb_bodyToChange.SelectedIndex >=0)
            {
                if (  MessageBoxResult.OK==MessageBox.Show("Sind Sie sicher, dass Sie den Körper entfernen möchten?", "Achtung", MessageBoxButton.OKCancel))
                {
                    main.world.CelestrialBodies.RemoveAt(cbb_bodyToChange.SelectedIndex);
                    AddBodiesToComboBox();
                    txt_Name_Edit.Text = null;
                    txt_Mass_Edit.Text = null;
                    txt_Position_Edit.Text = null;
                    txt_Radius_Edit.Text = null;
                    txt_Velocity_Edit.Text = null;

                }
            }
        }

        //Fügt einen Körper mit den eingegebenen Daten hinzu.
        private void btnHinzufügen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var body = new CelestrialBody()
                {
                    Name = txt_Name_New.Text,
                    Mass = float.Parse(txt_Mass_New.Text),
                    Radius = float.Parse(txt_Radius_New.Text),
                    Position = new Vector3D(getNumberFromText(txt_Position_New.Text, 0), getNumberFromText(txt_Position_New.Text, 1), getNumberFromText(txt_Position_New.Text, 2)),
                    Velocity = new Vector3D(getNumberFromText(txt_Velocity_New.Text, 0), getNumberFromText(txt_Velocity_New.Text, 1), getNumberFromText(txt_Velocity_New.Text, 2)),
                    BodyColor = Color.FromRgb((byte)Slider_Red.Value, (byte)Slider_Green.Value, (byte)Slider_Blue.Value),
                };
                if (MessageBoxResult.Yes == MessageBox.Show("Sind Sie sicher, dass Sie den Körper hinzufügen möchten?", "Bestätigung", MessageBoxButton.YesNo))
                {
                    main.world.CelestrialBodies.Add(body);
                    AddBodiesToComboBox();
                }
            }
            catch
            {
                MessageBox.Show("Der Körper konnte nicht hinzugefügt werden. Bitte vergewissern Sie sich, dass alle Daten im richtigen Format eingegeben wurden.");
                return;
            }
        }

        //Gibt die Zahlen am gewünschten Index eines strings zurück. Falls dies nicht geht, wird 0 zurückgegeben
        private float getNumberFromText(string Numbers, int i)
        {
            float Number;
            char[] splitSymbols = new char[] { ' ', ';', ',' };
            string[] singleNumbers = Numbers.Split(splitSymbols, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                Number = float.Parse(singleNumbers[i]);
                return Number;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        //Die Daten des gewählten Körpers werden in die Textfelder (und Slider) geladen.
        private void cbb_bodyToChange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                main.AnimationRunning = false;
                txt_Mass_Edit.Text = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Mass.ToString();
                txt_Name_Edit.Text = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Name.ToString();
                txt_Position_Edit.Text = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Position.ToString();
                txt_Velocity_Edit.Text = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Velocity.ToString();
                txt_Radius_Edit.Text = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Radius.ToString();
                Slider_Blue.Value = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].BodyColor.B;
                Slider_Red.Value = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].BodyColor.R;
                Slider_Green.Value = main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].BodyColor.G;
            }
            catch (Exception)
            {

            }
        }

        //Die Daten des Körpers werden mit den neuen Daten überschrieben.
        private void btn_SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Name = txt_Name_Edit.Text;
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Radius = float.Parse(txt_Radius_Edit.Text);
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Position = new Vector3D(getNumberFromText(txt_Position_Edit.Text, 0), getNumberFromText(txt_Position_Edit.Text, 1), getNumberFromText(txt_Position_Edit.Text, 2));
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Velocity = new Vector3D(getNumberFromText(txt_Velocity_Edit.Text, 0), getNumberFromText(txt_Velocity_Edit.Text, 1), getNumberFromText(txt_Velocity_Edit.Text, 2)); ;
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].Mass = float.Parse(txt_Mass_Edit.Text);
                main.world.CelestrialBodies[cbb_bodyToChange.SelectedIndex].BodyColor = Color.FromRgb((byte)Slider_Red.Value, (byte)Slider_Green.Value, (byte)Slider_Blue.Value);
            }
            catch(Exception)
            {
                MessageBox.Show("Es konnten nicht alle Änderungen gespeichert werden. Bitte vergewissern Sie sich, dass alles Richtig eingegeben wurde.");
            }
            int lastIndex = cbb_bodyToChange.SelectedIndex;
            AddBodiesToComboBox();
            cbb_bodyToChange.SelectedIndex = lastIndex;
        }

        //Die folgenden 9 Events steuern die Farbgebung eines Körpers (Bearbeiten oder Neu)
        private void ColorUpdate()
        {
            lbl_color.Background = new SolidColorBrush(Color.FromRgb((byte)Slider_Red.Value, (byte)Slider_Green.Value, (byte)Slider_Blue.Value));
        }

        private void Slider_Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdate();
        }        

        private void Slider_Green_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdate();
        }

        private void Slider_Blue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdate();
        }


        private void ColorUpdateNew()
        {
            lbl_color_New.Background = new SolidColorBrush(Color.FromRgb((byte)Slider_Red_New.Value, (byte)Slider_Green_New.Value, (byte)Slider_Blue_New.Value));
        }

        private void Slider_Green_New_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdateNew();
        }        

        private void Slider_Blue_New_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdateNew();
        }

        private void Slider_Red_New_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ColorUpdateNew();
        }
    }
}
