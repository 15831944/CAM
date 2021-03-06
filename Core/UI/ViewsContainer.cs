﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CAM
{
    public static class ViewsContainer
    {
        private static readonly Dictionary<Type, Type> _viewTypes;
        private static readonly Dictionary<Type, dynamic> _views = new Dictionary<Type, dynamic>();

        static ViewsContainer() => _viewTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Select(p => new { Type = p, DataInterface = p.GetInterfaces().SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDataView<>)) })
                .Where(p => p.Type.IsClass && p.Type.BaseType == typeof(UserControl) && p.DataInterface != null)
                .ToDictionary(p => p.DataInterface.GetGenericArguments().Single(), p => p.Type);

        public static void BindData(dynamic data, Control owner)
        {
            var dataType = data.GetType();
            if (!_viewTypes.ContainsKey(dataType))
                throw new Exception($"Не найдено представление для типа {dataType}");
            if (!_views.TryGetValue(dataType, out dynamic view))
            {
                view = Activator.CreateInstance(_viewTypes[dataType]);
                view.Dock = DockStyle.Fill;
                owner.Controls.Add(view);
                _views.Add(dataType, view);
            }
            view.BindData(data);
            view.Show();
            view.BringToFront();
        }
    }
}
