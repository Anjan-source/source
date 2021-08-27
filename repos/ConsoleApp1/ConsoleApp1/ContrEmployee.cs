using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    class ContrEmployee: BaseEmployee
    {

        public override string GetEmpPermissions()
        {

            return "Contract Employee";
        }

        public override void EmpSave()
        {

        }
    }
}
