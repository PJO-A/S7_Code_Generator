using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using listbox = System.Windows.Controls.ListBox;


namespace OpennessGenerator.Constructor
{
    public class Controller
    {
        public struct _Device
        {
            public string name;
            public string identifier;
            public string IP;
            public _Device(string _name, string _identifier, string _IP)
            {
                name = _name;
                identifier = _identifier;
                IP = _IP;
            }
        }
        public struct _IoCard
        {
            public string name;
            public string position;
            public string identifier;
            public _IoCard(string _name, string _position, string _identifier)
            {
                name = _name;
                position = _position;
                identifier = _identifier;
            }
        }

        public static IList<Tuple<_Device, IList<_IoCard>>> Device;
        public static listbox lbMessage;

        public static void ReadSettings(listbox _lbMessage)
        {
            lbMessage = _lbMessage;
            string appBaseDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            string filename = appBaseDir + "\\controller.csv";
            if (File.Exists(filename))
            {
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " Could not locate File: " + filename);
                return;
            }
            try
            {
                var reader = new StreamReader(filename);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    IList<_IoCard> iOC = new List<_IoCard>();
                    var holdD = new _Device(values[0], values[1], values[2]);
                    for (int i = 1; i < values.Length / 2; i++) 
                    { 
                        var holdIO  = new _IoCard(values[i*2+1], values[i*2+2], i.ToString());
                        iOC.Add(holdIO);
                    }
                    Device.Add(new Tuple<_Device, IList<_IoCard>>(holdD, iOC));
                }
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + "File objects.csv was read ");
            }
            catch (Exception ex)
            {
                lbMessage.Items.Insert(0, DateTime.Now.ToString() + " " + ex.Message);
            }
        }
    }
}
