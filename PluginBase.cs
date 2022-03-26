using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class PluginBase
{
    public abstract string[] Import();

    public abstract byte[] Export(string[] Strings);
}
