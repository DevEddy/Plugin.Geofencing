using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarcelloDB;
using MarcelloDB.Collections;
using System.Linq;
using System.IO;

namespace Plugin.Geofencing
{
    public class MarcelloDatabase
    {
        static MarcelloDatabase _instance;
        public static MarcelloDatabase Current => _instance = _instance ?? new MarcelloDatabase();

        MarcelloDB.netfx.Platform _platform;
        public MarcelloDB.netfx.Platform Platform => _platform = _platform ?? new MarcelloDB.netfx.Platform();

        string _dbPath;
        public string DbPath => _dbPath = _dbPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Geofencing");

        public Session Session { get; private set; }
        public CollectionFile CollectionFile { get; private set; }
        public string DatabaseName { get; } = "plugingeofencing.data";

        MarcelloDatabase()
        {
            if (!Directory.Exists(DbPath))
                Directory.CreateDirectory(DbPath);
            
            Session = new Session(Platform, DbPath);
            CollectionFile = Session[DatabaseName];
        }

        public Collection<T, string> GetCollection<T>() where T : IObjectIdentifier
        {
            try
            {
                var collectionName = $"{typeof(T).Name}s";
                return CollectionFile.Collection<T, string>(collectionName, x => x.Identifier);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting collection {ex.Message}");
            }
        }

        public IEnumerable<T> GetAll<T>() where T : IObjectIdentifier
        {
            try
            {
                var collection = GetCollection<T>();
                return collection.All;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in getting all {ex.Message}");
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : IObjectIdentifier
        {
            return await Task.Run(() =>
            {
                return GetAll<T>();   
            }).ConfigureAwait(false);
        }

        public async Task<T> Get<T>(string id) where T : IObjectIdentifier, new()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var collection = GetCollection<T>();
                    return collection.Find(id);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error getting item with id {id}, {ex.Message}");
                }
            }).ConfigureAwait(false);
        }

        public string Save<T>(T item) where T : IObjectIdentifier
        {
            try
            {
                var collection = GetCollection<T>();

                if (string.IsNullOrEmpty(item.Identifier))
                {
                    item.Identifier = Guid.NewGuid().ToString();
                }
                collection.Persist(item);
                Session.Transaction(() => { }); // BUGFIX
                return item.Identifier;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error persisting object {ex.Message}");
            }
        }

        public async Task<string> SaveAsync<T>(T item) where T : IObjectIdentifier
        {
            return await Task.Run(() =>
            {
                return Save<T>(item);
            }).ConfigureAwait(false);
        }

        public bool Delete<T>(T item) where T : IObjectIdentifier
        {
            return Delete<T>(item.Identifier);
        }

        public bool Delete<T>(string id) where T : IObjectIdentifier
        {
            try
            {
                var collection = GetCollection<T>();
                collection.Destroy(id);
                Session.Transaction(() => { }); // BUGFIX
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting item with id {id}. {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync<T>(T item) where T : IObjectIdentifier
        {
            return await DeleteAsync<T>(item.Identifier).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync<T>(string id) where T : IObjectIdentifier
        {
            return await Task.Run(() =>
            {
                return Delete<T>(id);
            }).ConfigureAwait(false);
        }

        public bool DeleteAll<T>() where T : IObjectIdentifier
        {
            foreach(var item in GetAll<T>().ToList())
                Delete(item);
            
            return true;
        }
    }
}
