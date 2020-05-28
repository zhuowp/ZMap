using System;
using System.Collections.Generic;
using System.Text;

namespace ZMap.Core
{
    public class PanoramaResourceConfig
    {
        public string Name { get; set; }
        public List<PanoramaLayer> Layers { get; set; }
    }
}
