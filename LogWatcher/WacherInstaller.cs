using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace LogWatcher
{
    [RunInstaller(true)]
    public partial class WacherInstaller : Installer
    {
        public WacherInstaller()
        {
            InitializeComponent();
        }
    }
}
