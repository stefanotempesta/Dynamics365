using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommonDataIntegrationPatterns.Correlation
{
    /*
     * Classes for mocking Entity Framework
    */

    internal class DataContext
    {
        public IList<Patient> Patients { get; set; }

        public IList<MedicalRecord> MedicalRecords { get; set; }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }
    }

    internal static class EFMockExtentions
    {
        public static Patient Find(this IList<Patient> obj, Guid id)
        {
            throw new NotImplementedException();
        }

        public static MedicalRecord Find(this IList<MedicalRecord> obj, Guid id)
        {
            throw new NotImplementedException();
        }
    }

    internal class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string name)
        {
            throw new NotImplementedException();
        }
    }
}