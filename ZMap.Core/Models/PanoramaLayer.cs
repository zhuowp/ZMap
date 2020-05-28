using System;
using System.Collections.Generic;
using System.Text;

namespace ZMap.Core
{
    public class PanoramaLayer
    {
        public int Level { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string ImageResourcePath { get; set; }
    }
}
