using System;
using System.IO;
using System.Globalization;
namespace ConsoleApp1
{
    class Program
    {
        static int i = 0;
        private static string directoryPath= @"C:\omnia\V3-Incidents\recyclebin\recyclebin\c95c2691-48ce-4fad-bd70-11fdedfcab57\AuditRiskFactor";

        private static void Main(string[] args)
        {
            C objc = new C();
            objc.foo();

            Sample3 obj = new Sample3();
            obj.Des();
            
            //string[] fileEntries = Directory.GetFiles(directoryPath);

            //foreach (var file_name in fileEntries)
            //{
            //    string fileName = file_name.Substring(directoryPath.Length + 1);
            //    Console.WriteLine(fileName);
            //}

            // if (TimeSpan.TryParse("0 0 0/1 1/1 * ? *", out TimeSpan value))
            // {
            //     TimeSpan timespan = value;
            // }

            // string fileName = @"C:\\Anjan\\Folder1\\SubFolder1\\Anjan.txt";
            // FileStream fs = File.Create(fileName);
            // string path = @"C:\\Anjan\\Folder1\\SubFolder1";
            // DirectoryInfo parentDir = Directory.GetParent(path);

            // path = parentDir + fs.Name;

            // File.Move(path, fileName);
            // //BaseEmployee objEmp;
            // //i = 1;

            // //if (i == 1)
            // //{
            // //    objEmp = new ParEmployee();
            // //}
            // //else
            // //{
            // //    objEmp = new ContrEmployee();
            // //}
            // BaseEmployee ObjEmp = new ParEmployee();
            // Console.WriteLine(ObjEmp.GetEmpDetails(1)); ;
            //// Console.WriteLine(objEmp.GetEmpPermissions());
        }
    }

    public class A
    {
        public void foo()
        {
            Console.WriteLine("class A");
        }
    }
    public class B : A
    {
        new void foo()
        {
            Console.WriteLine("class B");
        }
    }
    public class C : B
    {
        new void foo()
        {
            Console.WriteLine("class C");
        }
    }
}
