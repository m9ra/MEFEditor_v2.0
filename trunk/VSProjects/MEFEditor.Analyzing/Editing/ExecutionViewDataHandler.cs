using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Analyzing.Editing
{
    class ExecutionViewDataHandler
    {
        Dictionary<object, Dictionary<Type, ExecutionViewData>> _data = new Dictionary<object, Dictionary<Type, ExecutionViewData>>();

        /// <summary>
        /// Commit all contained view data
        /// </summary>
        internal void Commit()
        {
            foreach (var viewData in _data.Values)
            {
                foreach (var data in viewData.Values)
                {
                    data.Commit();
                }
            }
        }

        /// <summary>
        /// Fill local copy of view data from given handler
        /// </summary>
        /// <param name="handler">Handler storing data for local copy</param>
        internal void FillFrom(ExecutionViewDataHandler handler)
        {
            foreach (var dataPair in handler._data)
            {
                var container=new Dictionary<Type,ExecutionViewData>();
                _data[dataPair.Key]=container;
                foreach (var typePair in dataPair.Value)
                {
                    container[typePair.Key] = typePair.Value.Clone();
                }
            }
        }

        /// <summary>
        /// Get data stored in current view for given key and type.
        /// If no matching data are found, new data is created via provider.
        /// </summary>
        /// <typeparam name="T">Type of searched data</typeparam>
        /// <param name="key">Key of data - because multiple sources can store data with same type</param>
        /// <param name="provider">Provider used for data creation</param>
        /// <returns>Stored data, created data, or null if the data is not found and provider is not present</returns>
        internal T Data<T>(object key, ViewDataProvider<T> provider)
            where T : ExecutionViewData
        {
            var type = typeof(T);
            var canCreate = provider != null;

            Dictionary<Type, ExecutionViewData> keyData;
            if (!_data.TryGetValue(key, out keyData))
            {
                if (!canCreate)
                    return null;

                _data[key] = keyData = new Dictionary<Type, ExecutionViewData>();
            }

            ExecutionViewData viewData;
            if (!keyData.TryGetValue(type, out viewData))
            {
                if (!canCreate)
                    return null;

                keyData[type] = viewData = provider();
            }

            return viewData as T;
        }
    }
}
