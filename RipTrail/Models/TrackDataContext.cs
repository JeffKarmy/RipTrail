using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace RipTrail.Models
{
    class TrackDataContext : DataContext
    {

        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/Tracks.sdf";

        // Pass the connection string to the base class.
        public TrackDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a single table for the to-do items.
        public Table<TrackDataBase> TrackTable;

    } // End class
} // End namespace
