using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonStringClear_unitTests
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            clearJsonString("'SEGGERJ-LinkCommanderV6.12(CompiledNov25201618:08:52)';'DLLversionV6.12,compiledNov25201618:08:26';'';'';'Scriptfilereadsuccessfully.';'Processingscriptfile...';'';'J-Linkconnectionnotestablishedyetbutrequiredforcommand.';'ConnectingtoJ-LinkviaUSB...O.K.';'Firmware:J-LinkV9compiledDec16201615:34:10';'Hardwareversion:V9.30';'S/N:269306804';'License(s):FlashBP,GDB';'OEM:SEGGER-EDU';'VTref--3.261V';'';'Selecting500kHzastargetinterfacespeed';'';'SelectingSWDascurrenttargetinterface.';'';'Device'ATSAMD09D14'selected.';'';'';'';'******Error:SAMD(connect):Failed.Couldnotidentifydevice.';'';'';'******Error:SAMD(connect):Failed.Couldnotidentifydevice.';'';'Cannotconnecttotarget.';'';'';'Scriptprocessingcompleted.';'';'',stage:etapaF,message:Nãofoipossívelgravarofirmwaredeteste\"");
            clearJsonString("{'SEGGERJ-LinkCommanderV6.12(CompiledNov25201618:08:52)';'DLLversionV6.12,compiledNov25201618:08:26';'';'';'Scriptfilereadsuccessfully.';'Processingscriptfile...';'';'J-Linkconnectionnotestablishedyetbutrequiredforcommand.';'ConnectingtoJ-LinkviaUSB...O.K.';'Firmware:J-LinkV9compiledDec16201615:34:10';'Hardwareversion:V9.30';'S/N:269306804';'License(s):FlashBP,GDB';'OEM:SEGGER-EDU';'VTref--3.256V';'';'Selecting500kHzastargetinterfacespeed';'';'SelectingSWDascurrenttargetinterface.';'';'Device'ATSAMD09D14'selected.';'';'';'';'******Error:SAMD(connect):Failed.Couldnotidentifydevice.';'';'';'******Error:SAMD(connect):Failed.Couldnotidentifydevice.';'';'Cannotconnecttotarget.';'';'';'Scriptprocessingcompleted.';'';'',stage:etapaF,message:Nãofoipossívelgravarofirmwaredeteste\"}");
        }


        private string clearJsonString(string json)
        {
            StringBuilder result = new StringBuilder();
            bool quotes = false;

            char oldAtt = (char)0;
            foreach (char att in json)
            {
                if ((att == '\"') && (oldAtt != '\\'))
                    quotes = !quotes;

                if (!quotes)
                {
                    if (!"\r\n\t ".Contains(att))
                        result.Append(att);
                }
                else
                {
                    result.Append(att);
                }
            }

            //result = result.Replace("\r", "\\r").Replace("\n", "\\n");
            return result.ToString();
        }
    }

}
