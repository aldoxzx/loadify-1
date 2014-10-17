﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.View;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshDisposal>, IHandle<DownloadRequestEvent>
    {
        private ObservableCollection<PlaylistViewModel> _Playlists = new ObservableCollection<PlaylistViewModel>();
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get
            {
                if (SearchTerm.Length == 0) return _Playlists;

                var matchingPlaylists = new ObservableCollection<PlaylistViewModel>();
                foreach (var playlist in _Playlists)
                {
                    var matchingTracks =
                        new ObservableCollection<TrackViewModel>(playlist.Tracks
                                                                .Where(track => track.ToString()
                                                                .Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
                    if (matchingTracks.Count != 0)
                    {
                        matchingPlaylists.Add(new PlaylistViewModel(playlist)
                        {
                            Tracks = matchingTracks
                        });
                    }
                }

                return matchingPlaylists;
            }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        private string _SearchTerm = "";
        public string SearchTerm
        {
            get { return _SearchTerm; }
            set
            {
                if (_SearchTerm == value) return;
                _SearchTerm = value;
                NotifyOfPropertyChange(() => SearchTerm);
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public PlaylistsViewModel(IEnumerable<PlaylistViewModel> playlistCollection, IEventAggregator eventAggregator) :
            base(eventAggregator)
        {
            _Playlists = new ObservableCollection<PlaylistViewModel>(playlistCollection);
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator) :
            this(new ObservableCollection<PlaylistViewModel>(), eventAggregator)
        { }

        public async void Handle(DataRefreshDisposal message)
        {
            var playlists = await message.Session.GetPlaylists();
            Playlists = new ObservableCollection<PlaylistViewModel>(playlists.Select(playlist => new PlaylistViewModel(playlist, _EventAggregator)));
        }

        public void Handle(DownloadRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DownloadEvent(message.Session, _Playlists.SelectMany(playlist => playlist.SelectedTracks)));
        }
    }
}
