using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mpeg4Tagging.WindowsMedia;

namespace Mpeg4TagEditorDemo
{
    public class CommonAttributes
    {
        private Dictionary<string, ContentDescriptor> zuneTags = new Dictionary<string, ContentDescriptor>();

        protected void SetAttribute(string name, object value)
        {
            if (value == null)
            {
                if (zuneTags.ContainsKey(name))
                    zuneTags.Remove(name);
                return;
            }

            ContentDescriptor descriptor = new ContentDescriptor() { Name = name };
            descriptor.SetValue(value);

            if (zuneTags.ContainsKey(name))
                zuneTags[name] = descriptor;
            else
                zuneTags.Add(name, descriptor);
        }

        public ContentDescriptor[] ZuneMetaData
        {
            get
            {
                return zuneTags.Values.ToArray();
            }
        }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                SetAttribute("Title", title);
            }
        }

        /* public string[] Artists
        {
            get;
            set;
        } */

        private string albumArtist;
        public string AlbumArtist
        {
            get
            {
                return albumArtist;
            }
            set
            {
                albumArtist = value;
                SetAttribute("WM/AlbumArtist", albumArtist);
            }
        }

        private string album;
        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                album = value;
                SetAttribute("WM/AlbumTitle", album);
            }
        }

        /* private string albumArt;
        public string AlbumArt
        {
            get
            {
                return albumArt;
            }
            set
            {
                albumArt = value;
                SetAttribute("WM/Picture", album);
            }
        } */

        private byte? trackNumber;
        public byte? TrackNumber
        {
            get
            {
                return trackNumber;
            }
            set
            {
                trackNumber = value;
                SetAttribute("WM/TrackNumber", trackNumber.HasValue ? trackNumber.ToString() : null);
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                SetAttribute("WM/SubTitleDescription", description);
            }
        }

        private string author;
        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                album = value;
                SetAttribute("Author", author);
            }
        }

        private int? year;
        public int? Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
                SetAttribute("WM/Year", year.HasValue ? year.ToString() : null);
            }
        }

        private string genre;
        public string Genre
        {
            get
            {
                return genre;
            }
            set
            {
                genre = value;
                SetAttribute("WM/Genre", genre);
            }
        }

        private string series;
        public string Series
        {
            get
            {
                return series;
            }
            set
            {
                series = value;
                SetAttribute("Title", title);
            }
        }

        private string episodeTitle;
        private string EpisodeTitle
        {
            get
            {
                return episodeTitle;
            }
            set
            {
                episodeTitle = value;
                SetAttribute("WM/SubTitle", episodeTitle);
            }
        }

        private MediaKind? mediaKind;
        public MediaKind? MediaKind
        {
            get
            {
                return mediaKind;
            }
            set
            {
                mediaKind = value;

                switch (mediaKind.Value)
                {
                    case Mpeg4TagEditorDemo.MediaKind.Movie:
                    case Mpeg4TagEditorDemo.MediaKind.MusicVideo:
                    case Mpeg4TagEditorDemo.MediaKind.TVShow:
                    case Mpeg4TagEditorDemo.MediaKind.Video:
                        SetAttribute("WM/MediaClassPrimaryID", new Guid("DB9830BD-3AB3-4FAB-8A37-1A995F7FF74B"));
                        break;
                    default:
                        SetAttribute("WM/MediaClassPrimaryID", null);
                        break;
                }

                switch (mediaKind.Value)
                {
                    case Mpeg4TagEditorDemo.MediaKind.Movie:
                        SetAttribute("WM/MediaClassSecondaryID", new Guid("A9B87FC9-BD47-4BF0-AC4F-655B89F7D868"));
                        break;
                    case Mpeg4TagEditorDemo.MediaKind.MusicVideo:
                        SetAttribute("WM/MediaClassSecondaryID", new Guid("E3E689E2-BA8C-4330-96DF-A0EEEFFA6876"));
                        break;
                    case Mpeg4TagEditorDemo.MediaKind.TVShow:
                        SetAttribute("WM/MediaClassSecondaryID", new Guid("BA7F258A-62F7-47A9-B21F-4651C42A000E"));
                        break;
                    default:
                        SetAttribute("WM/MediaClassSecondaryID", null);
                        break;
                }
            }
        }
    }

    public enum MediaKind
    {
        MusicVideo,
        Movie,
        TVShow,
        Podcast,
        Video,
        Audio
    }
}
