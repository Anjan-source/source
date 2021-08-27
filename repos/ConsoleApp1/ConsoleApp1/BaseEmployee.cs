using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public abstract class BaseEmployee
    {
        public abstract void EmpSave();
        

        public Employee GetEmpDetails(int ID)
        {
            Employee objEmp = new Employee();
            if (ID == 1)
            {
                objEmp.ID = 1;
                objEmp.Name = "Anjna";
                objEmp.Dept = "Parmenent";
                objEmp.Salary = 20000;

            }
            else
            {
                objEmp.ID = 1;
                objEmp.Name = "Anjan2";
                objEmp.Dept = "Contractor";
                objEmp.Salary = 20000;
            }

            return objEmp;
        }

        public IList<Employee> GetEmployees()
        {
            IList<Employee> objList = new List<Employee>();
            return objList;
        }

        public abstract string GetEmpPermissions();
       


    }
}
