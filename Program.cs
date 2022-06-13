/*
Code to download active directory users from Windows Domain and store them to sql table using sqlbulkcopy
*/
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Data;
using System.Data.SqlClient;
namespace ActiveDirectory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            AdUpload();
            
        }

        public static void AdUpload()
        {
            string con_string = @"Server=SNATARAJAN-VM1\SNATARAJAN;Database=Sivabose_Demo;Integrated Security=true;";
            DataTable dt = new DataTable();
            dt.Columns.Add("account_name", typeof(string));
            dt.Columns.Add("employee_id", typeof(string));
            dt.Columns.Add("first_name", typeof(string));
            dt.Columns.Add("last_name", typeof(string));
            dt.Columns.Add("full_name", typeof(string));
            dt.Columns.Add("department", typeof(string));
            dt.Columns.Add("email", typeof(string));
            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "DomainName")) // repace the value "DomainName" with respective domain name from your organization
            {
                UserPrincipal userPrincipal = new UserPrincipal(principalContext);
                userPrincipal.Enabled = true;
                using (PrincipalSearcher principalSearcher = new PrincipalSearcher(new UserPrincipal(principalContext)))
                {
                    principalSearcher.QueryFilter = userPrincipal;
                    foreach (var result in principalSearcher.FindAll())
                    {
                        DirectoryEntry? directoryEntry = result.GetUnderlyingObject() as DirectoryEntry;
                        DataRow row = dt.NewRow();
                        row["account_name"] = directoryEntry.Properties["sAMAccountName"].Value;
                        row["employee_id"] = directoryEntry.Properties["employeeID"].Value;
                        row["first_name"] = directoryEntry.Properties["givenName"].Value;
                        row["last_name"] = directoryEntry.Properties["sn"].Value;
                        row["full_name"] = directoryEntry.Properties["cn"].Value;
                        row["department"] = directoryEntry.Properties["department"].Value;
                        row["email"] = directoryEntry.Properties["mail"].Value;
                        dt.Rows.Add(row);
                    }
                }
            }
            using (SqlConnection cn = new SqlConnection(con_string))
            {
                cn.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(cn))
                {
                    bulkCopy.DestinationTableName = "dbo.employee";
                    bulkCopy.BatchSize = 3000;
                    bulkCopy.WriteToServer(dt);

                }
            }
        }

        public static void AdSearch()
        {
            using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "RMC"))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, "svenkataraman"))
                {
                    if (user != null)
                    {
                        DirectoryEntry? directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                        foreach (string propname in directoryEntry.Properties.PropertyNames)
                        {
                            Console.WriteLine("{0}:{1}", propname, directoryEntry.Properties[propname].Value);
                        }
                    }
                }
            }
        }
    }
}
