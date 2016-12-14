using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApkTools {
    public class ApkInfo {
        public string package;
        public string versionName;
        public string appName;
        public string icon;
        public string sign;

        public void Parse(string line) {
            if (line.StartsWith("package:")) {
                SplitPackage(line);
            } else if (line.StartsWith("application:")) {
                SplitNameIcon(line);
            }
        }

        public void SplitPackage(string line) {
            string[] ss = line.Split(' ');
            package = ss[1].Substring(6, ss[1].Length - 7);
            string version = ss[2].Substring(13, ss[2].Length - 14);
            versionName = version + " | " + ss[3].Substring(13, ss[3].Length - 14);
        }

        public void SplitNameIcon(string line) {
            string[] ss = line.Split(' ');
            appName = ss[1].Substring(7, ss[1].Length - 8);
            icon = ss[2].Substring(6, ss[2].Length - 7);
        }
    }
}
