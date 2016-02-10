﻿using System.Collections.Generic;
using MALClient.ViewModels;

// ReSharper disable InconsistentNaming

namespace MALClient.Items
{
    /// <summary>
    ///     This class serves as a container for actual UI AnimeItem element.
    ///     It contains all values required to sort and filter those items before loading them.
    /// </summary>
    public class AnimeItemAbstraction
    {
        private readonly bool _firstConstructor;
        private readonly int allEps;

        private readonly SeasonalAnimeData data;
        private readonly int id;
        private readonly string img;
        private readonly int myEps;
        private readonly int myScore;
        private readonly int myStatus;
        private readonly string name;

        private AnimeItem _animeItem;
        public bool Loaded;
        public int AirDay = -1;
        public int AllEpisodes;

        private bool auth;
        private bool authSetEps;
        public float GlobalScore;
        public int Id;
        public int Index;
        public int MyEpisodes;
        public int MyScore;
        public int MyStatus;
        public string Title;

        private AnimeItemAbstraction(int id)
        {
            Id = id;
            VolatileDataCache data;
            if (!DataCache.TryRetrieveDataForId(Id, out data)) return;
            AirDay = data.DayOfAiring;
            GlobalScore = data.GlobalScore;
        }

        //Two constructors depending on original init
        public AnimeItemAbstraction(bool auth, string name, string img, int id, int myStatus, int myEps, int allEps,
            int myScore) : this(id)
        {
            this.auth = auth;
            this.name = name;
            this.img = img;
            this.id = id;
            this.myStatus = MyStatus = myStatus;
            this.myEps = MyEpisodes = myEps;
            this.allEps = allEps;
            this.myScore = MyScore = myScore;
            _firstConstructor = true;

            Title = name;
        }

        public AnimeItemAbstraction(SeasonalAnimeData data)
            : this(data.Id)
        {
            this.data = data;

            Title = data.Title;
            GlobalScore = data.Score;
            Index = data.Index;

            MyStatus = (int) AnimeStatus.AllOrAiring;
        }

        public AnimeItem AnimeItem
        {
            get
            {
                if (Loaded)
                    return _animeItem;
                ViewModel = LoadElementModel();
                _animeItem = LoadElement();
                return _animeItem;
            }
        }

        private AnimeItemViewModel _viewModel;

        public AnimeItemViewModel ViewModel
        {
            get
            {
                if (Loaded)
                    return _viewModel;
                ViewModel = LoadElementModel();
                _animeItem = LoadElement();
                return _viewModel;
            }
            private set { _viewModel = value; }
        }

        public bool TryRetrieveVolatileData(bool force = false)
        {
            if (GlobalScore != 0 && !force)
                return true;
            VolatileDataCache data;
            if (!DataCache.TryRetrieveDataForId(Id, out data)) return false;
            AirDay = data.DayOfAiring;
            GlobalScore = data.GlobalScore;
            return true;
        }

        public void SetAuthStatus(bool status, bool eps)
        {
            if (Loaded)
                ViewModel.SetAuthStatus(status, eps);
            else
            {
                auth = status;
                authSetEps = eps;
            }
        }

        //Load UIElement
        private AnimeItemViewModel LoadElementModel()
        {
            return _firstConstructor
                ? new AnimeItemViewModel(auth, name, img, id, myStatus, myEps, allEps, myScore, this, authSetEps)
                : new AnimeItemViewModel(data, this);
        }

        private AnimeItem LoadElement()
        {
            Loaded = true;
            return new AnimeItem(ViewModel);
        }
    }
}