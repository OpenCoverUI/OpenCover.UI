using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.TestDiscoverer
{
    internal interface IDiscoverer
    {
        List<TestClass> Discover();
    }
}
