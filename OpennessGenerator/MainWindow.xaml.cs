using OpennessGenerator.Constructor;
using System.Windows;
using Siemens.Engineering;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;



namespace OpennessGenerator
{
    public partial class MainWindow : Window
    {
        #region Field
        private TiaPortal portal = null;
        private Project project = null; 
        private Device device = null;
        private DirectoryInfo path = null;
        private IoSystem system = null;
        private IList<Device> subDevice = null;
        private IList<Device> Devices = null;
        private IList<addDevice> addDev = new List<addDevice>();
        private addDevice add;

        public struct addDevice
        {
            public string name;
            public string author;
            public string ipAddress;
            public string identifier;
            public string comment;
        }

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            ObjectIdentifier.ReadSettings(lbMessage);
            Controller.ReadSettings(lbMessage);

            tbPlcName.Text = "PLC_ofmSystems";
            tbPlcAuthor.Text = "ofmSystems";
            tbPlcIPAddress.Text = "192.168.10.20";
            tbPlcIdentifier.Text = "PLC_CPU";

            tbHmiName.Text = "PLC_ofmSystems";
            tbHmiAuthor.Text = "ofmSystems";
            tbPlcIPAddress.Text = "192.168.10.20";
            tbPlcIdentifier.Text = "PLC_CPU";

            tbComment.Text = "Standard comment";
            tbPath.Text = "C:\\Users\\TIA V19\\source\\repos\\OpennessGenerator\\Siemens";
            cbDelete.IsChecked = true;

        }
        private void createDevices(Project project)
        {
            Devices = new List<Device>();
            NetworkInterface network = null;

            add = new addDevice
            {
                name = tbHmiName.Text,
                author = tbHmiAuthor.Text,
                ipAddress = tbHmiIPAddress.Text,
                identifier = tbHmiIdentifier.Text,
                comment = tbComment.Text,
            };
            addDev.Add(add);

            add = new addDevice
            {
                name = tbPlcName.Text,
                author = tbPlcAuthor.Text,
                ipAddress = tbPlcIPAddress.Text,
                identifier = tbPlcIdentifier.Text,
                comment = tbComment.Text,
            };
            addDev.Add(add);
        }

        private void btnGenHw_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReloadCsv_Click(object sender, RoutedEventArgs e)
        {
            ObjectIdentifier.ReadSettings(lbMessage);
            Controller.ReadSettings(lbMessage);
        }
    }
}
