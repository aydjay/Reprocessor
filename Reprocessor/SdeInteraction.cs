using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Reprocessor.Models;

namespace Reprocessor
{
    public class SdeInteraction : IDisposable
    {
        private readonly SQLiteConnection _connection;

        public SdeInteraction()
        {
            _connection = new SQLiteConnection(@"Data Source=C:\temp\sqlite-latest.sqlite");
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Close();
        }

        public IEnumerable<EveItem> GetItems(string filter = "")
        {
            var rtnList = new List<EveItem>();
            var query = @"SELECT t.typeID
                             , t.typeName
                            FROM invTypes as t
                            inner JOIN invTypeMaterials AS m ON t.typeID = m.typeID";

            if (string.IsNullOrEmpty(filter) == false)
            {
                query += " AND (typeName LIKE @filter)";
            }

            query += " order by typeName asc";

            var data = GetDataTableFromSde(query, new Tuple<string, object>("@filter", "%" + filter + "%"));

            foreach (DataRow item in data.Rows)
            {
                rtnList.Add(new EveItem {TypeId = item["typeID"].ToString(), TypeName = item["typeName"].ToString()});
            }

            return rtnList;
        }

        public DataTable GetReprocessDetails(int typeId)
        {
            return GetDataTableFromSde($@"SELECT * FROM invTypeMaterials where typeid = @typeId", new Tuple<string, object>("@typeId", typeId));
        }

        public string GetItemNameFromTypeId(string typeId)
        {
            var query = $"Select typeName from invTypes where typeId = @typeId";
            return GetScalarFromSde(query, new Tuple<string, object>("@typeId", typeId)).ToString();
        }

        public int GetPortionSize(int typeId)
        {
            var query = "select portionSize from invTypes where typeId = @typeId";
            return int.Parse(GetScalarFromSde(query, new Tuple<string, object>("@typeId", typeId)).ToString());
        }
        
        private DataTable GetDataTableFromSde(string query, params Tuple<string,object>[] queryParams )
        {
            var dt = new DataTable();


            using (var fmd = _connection.CreateCommand())
            {
                fmd.CommandText = query;
                foreach (var tuple in queryParams)
                {
                    fmd.Parameters.AddWithValue(tuple.Item1, tuple.Item2);
                }
                    
                fmd.CommandType = CommandType.Text;
                    
                var reader = fmd.ExecuteReader();
                dt.Load(reader);
            }
            

            return dt;
        }

        private object GetScalarFromSde(string query, params Tuple<string,object>[] queryParams )
        {
            using (var fmd = _connection.CreateCommand())
            {
                fmd.CommandText = query;
                foreach (var tuple in queryParams)
                {
                    fmd.Parameters.AddWithValue(tuple.Item1, tuple.Item2);
                }

                fmd.CommandType = CommandType.Text;
                return fmd.ExecuteScalar();
            }
        }

        public int RegionIdFromName(string regionName)
        {
            return int.Parse(GetScalarFromSde("Select regionID from mapRegions where regionName=@regionName", new Tuple<string, object>("@regionName", regionName)).ToString());
        }


        public Dictionary<int, string> GetAllRegions()
        {
            
            var data = GetDataTableFromSde("Select regionId, regionName from mapRegions");

            var rtnDictionary = new Dictionary<int, string>();

            foreach (DataRow dataRow in data.Rows)
            {
                rtnDictionary.Add (Convert.ToInt32(dataRow["regionId"].ToString()), dataRow["regionName"] as string);
            }

            return rtnDictionary;
        }
    }
}