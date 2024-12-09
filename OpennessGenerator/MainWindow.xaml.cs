﻿using OpennessGenerator.Constructor;
using System.Windows;
using Siemens.Engineering;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System;
using System.Linq;
using Siemens.Engineering.HW.Utilities;



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

        private void GenerateDevice()
        {
            if(Directory.Exists(tbPath.Text + tbPlcName.Text))
            {
                try
                {
                    if(cbDelete.IsChecked == true)
                    {
                        Directory.Delete(tbPath.Text + tbPlcName.Text, true);
                        lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Project " + tbPlcName.Text + " was detected ");
                    }
                }
                catch(Exception e)
                {
                    lbMessage.Items.Insert(0, DateTime.Now.ToString()+ " " + e.Message);
                }
                return;
            }
            using (portal = new TiaPortal(TiaPortalMode.WithUserInterface))
            {
                path = new DirectoryInfo(tbPath.Text);

                project = portal.Projects.Create(path, tbPlcName.Text);
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Project " + project.Name + " was created");

                IoDevice(Controller.Device);
                createDevices(project);

                foreach(var a in project.UngroupedDevicesGroup.Devices)
                {
                    FindAttributeSet(a.DeviceItems, "Author", tbPlcAuthor.Text);
                    FindAttributeSet(a.DeviceItems, "Comment", tbComment.Text + " " + a.Name);
                }

                AddToSubnet(Controller.Device, system);
                SaveProject();
            }
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

            foreach(var a in addDev)
            {
                var device = project.Devices.CreateWithItem("OrderNumber: " + ObjectIdentifier.Identifier[a.identifier], a.name, a.name);
                Devices.Add(device);
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Device" + device.Name + " was created");

                FindAttributeSet(device.DeviceItems, "Suthor", tbPlcAuthor.Text);
                FindAttributeSet(device.DeviceItems, "Comment", tbComment.Text + " " + device.Name);

                network = FindNetworkInterface(device.DeviceItems);
                network.Nodes.Last().ConnectToSubnet(project.Subnets.Find("GlobalNetwork"));
                network.Nodes.Last().SetAttribute("Address", a.ipAddress);
            }
            var controller = network.IoControllers.First();
            if (controller == null) return;
            system = controller.CreateIoSystem("GeneralSystem");
        }

        private void SaveProject()
        {
            if(project != null)
            {
                project.Save();
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Project " + project.Name + " was saved");
            }
        }

        private void IoDevice(IList<Tuple<Controller._Device, IList<Controller._IoCard>>> devices)
        {
            subDevice = new List<Device>();
            var subnet = project.Subnets.Create("System:Subnet.Ethernet", "GlobalNetwork");
            foreach(var d in devices)
            {
                var device = project.UngroupedDevicesGroup.Devices.CreateWithItem("OrderNumber:" + ObjectIdentifier.Identifier[d.Item1.identifier], d.Item1.name, d.Item1.name);
                subDevice.Add(device);
                FindAttributeSet(device.DeviceItems, "Suthor", tbPlcAuthor.Text);
                FindAttributeSet(device.DeviceItems, "Comment", tbComment.Text + " " + device.Name);

                FillIoController(device, d.Item2);
                var network = FindNetworkInterface(device.DeviceItems);
                network.Nodes.Last().ConnectToSubnet(subnet);
            }
        }

        private void FillIoController(Device device, IList<Controller._IoCard> controller)
        {
            int index = 0;
            DeviceItem rail = FindRail(device.DeviceItems);
            foreach(var card in controller)
            {
                if (card.name.Equals(string.Empty)) continue;
                for (int i=index; i<controller.Count; i++)
                {
                    if (!rail.CanPlugNew("OrderNumber:" + ObjectIdentifier.Identifier[card.identifier], card.name + " " + card.position, i)) continue;
                    index = i;
                    rail.PlugNew("OrderNumber:" + ObjectIdentifier.Identifier[card.identifier], card.name + "_" + card.position, i);
                    break;
                }
            }
        }

        private void AddToSubnet(IList<Tuple<Controller. _Device, IList<Controller._IoCard>>> devices, IoSystem system)
        {
            foreach(var d in project.UngroupedDevicesGroup.Devices)
            {
                if(d.Name.Contains(tbPlcName.Text)) continue;

                var network = FindNetworkInterface(d.DeviceItems);
                network.IoConnectors.Last().ConnectToIoSystem(system);

                foreach(var v in devices)
                {
                    if (d.Name.Equals(v.Item1.name) && !v.Item1.IP.Equals(string.Empty))
                    {
                        network.Nodes.First().SetAttribute("Address", v.Item1.IP);
                    }
                }
            }
        }







        #region Helper Function

        private DeviceItem FindRail(DeviceItemComposition devices)
        {
            foreach(var item in devices)
            {
                if (item.Name.Contains("Rack")) return item;
                return FindRail(item.DeviceItems);
            }
            return null;
        }
        private NetworkInterface FindNetworkInterface(DeviceItemComposition devices)
        {
            NetworkInterface network = null;
            foreach (var device in devices)
            {
                if (network != null) continue;
                network = device.GetService<NetworkInterface>();
                if  (network != null)
                {
                    if (FindAttributeGet(device, "InterfaceType") != "Ethernet") network = null;
                    else if (network.Nodes == null) network = null;
                    else if (network.Nodes.Count == 0) network = null;
                }
                if (network == null) network = FindNetworkInterface(device.DeviceItems);
            }
            return network;
        }
        private string  FindAttributeGet(DeviceItem device, string attribute)
        {
            var list = device.GetAttributeInfos();
            foreach (var item in list) 
            {
                if (item.Name.Equals(attribute))
                {
                    return device.GetAttribute(attribute).ToString();
                }
            }
            return string.Empty;
        }

        private string FindAttributeSet(DeviceItemComposition devices, string attribute, object value)
        {
            foreach (var d in devices)
            {
                var list = d.GetAttributeInfos();
                foreach (var item in list)
                {
                    if (item.Name.Equals(attribute))
                    {
                        d.SetAttribute(attribute, value);
                    }
                }
            }
            return string.Empty;
        }

        #endregion

        #region Events
        private void btnGenHw_Click(object sender, RoutedEventArgs e)
        {
            if(project != null)
            {
                project.Save();
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Project " + project.Name + " was saved");
            }
        }

        private void btnReloadCsv_Click(object sender, RoutedEventArgs e)
        {
            ObjectIdentifier.ReadSettings(lbMessage);
            Controller.ReadSettings(lbMessage);
        }
        #endregion
    }
}
