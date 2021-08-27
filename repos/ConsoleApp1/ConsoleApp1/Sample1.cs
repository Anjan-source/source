using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class Sample1
    {

        public virtual string GetName()
        {
            return "Anjan";
        }
    }
    public class Sample2 : Sample1
    {

        public override string GetName()
        {
            return "Venky";
        }
    }
}
