using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using Analogy.LogViewer.Philips.CustomActions;
using Analogy.LogViewer.Philips.DataSources;
using System;
using System.Collections.Generic;

namespace Analogy.LogViewer.Philips.Factories
{
    public class ICAPFactory : IAnalogyFactory
    {
        public Guid FactoryID { get; } = new Guid("F0FD2BC8-5DA5-4A7D-8E09-4E4C411EDA0C");
        public string Title { get; } = "Philips ICAP BU Logs";
        public IAnalogyDataProvidersFactory DataProviders { get; }
        public IAnalogyCustomActionsFactory Actions { get; }
        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; } = new List<IAnalogyChangeLog>(0);
        public IEnumerable<string> Contributors { get; } = new List<string> { "Lior Banai" };
        public string About { get; } = "Created by Lior Banai";


        public ICAPFactory()
        {
            DataProviders = new ICAPDataSourcesFactory();
            Actions = new ICAPCustomActionFactory();
        }

    }

    public class ICAPDataSourcesFactory : IAnalogyDataProvidersFactory
    {
        public IEnumerable<IAnalogyDataProvider> Items { get; }
        public string Title { get; } = "Philips ICAP BU Data Sources";
        public ICAPDataSourcesFactory()
        {
            var dataSources = new List<IAnalogyDataProvider>();
            dataSources.Add(new OfflineICAPLog());
            Items = dataSources;
        }



    }

    public class ICAPCustomActionFactory : IAnalogyCustomActionsFactory
    {
        public string Title { get; } = "Philips ICAP BU Tools";
        public IEnumerable<IAnalogyCustomAction> Items => Actions;
        private List<IAnalogyCustomAction> Actions { get; }

        public ICAPCustomActionFactory()
        {
            Actions = new List<IAnalogyCustomAction>();
            Actions.Add(new FixCorruptedFilelAction());
            //Actions.Add(new SplunkAction());
            Actions.Add(new LogConfiguratorAction());
        }


    }

    //public class ICAPUserControlFactory : IAnalogyUserControlFactory
    //{
    //    public string Title { get; } = "Philips ICAP BU User Controls";
    //    public IEnumerable<IAnalogyUserControl> Items { get; }
    //    public ICAPUserControlFactory()
    //    {
    //        Items = new List<IAnalogyUserControl>(0);
    //    }

    //}
}
