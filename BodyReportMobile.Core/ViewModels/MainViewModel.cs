﻿using System;
using System.Windows.Input;
using System.Threading.Tasks;
using BodyReportMobile.Core.Message;
using System.Collections.Generic;
using Message;
using BodyReportMobile.Core.ServiceManagers;
using SQLite.Net;
using XLabs.Ioc;
using BodyReportMobile.Core.ViewModels.Generic;
using BodyReportMobile.Core.Manager;
using BodyReportMobile.Core.WebServices;
using BodyReportMobile.Core.Framework;
using Xamarin.Forms;
using BodyReportMobile.Core.Data;
using System.IO;

namespace BodyReportMobile.Core.ViewModels
{
	public class MainViewModel : BaseViewModel
	{
		public string MenuLabel { get; set;}
		public string ConfigurationLabel { get; set;}
		public string TrainingJournalLabel { get; set;}
		public string ChangeLanguageLabel { get; set;}
        public string _userProfilImage { get; set; }


        public string UserProfilImage
        {
            get { return _userProfilImage; }
            set
            {
                if (value != _userProfilImage)
                {
                    _userProfilImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _languageFlagImageSource;

		public string LanguageFlagImageSource
		{
			get { return _languageFlagImageSource; }
			set 
			{
				if (value != _languageFlagImageSource) {
					_languageFlagImageSource = value;
                    OnPropertyChanged();
				}
			}
		}

		private SQLiteConnection _dbContext;
        private IFileManager _fileManager;
        private string _userProfilLocalPath;

        public MainViewModel() : base()
        {
			_dbContext = Resolver.Resolve<ISQLite> ().GetConnection ();
            _fileManager = Resolver.Resolve<IFileManager>();
            ShowDelayInMs = 0;

            _userProfilLocalPath = Path.Combine(_fileManager.GetDocumentPath(), "userprofil");
            if (!_fileManager.DirectoryExist(_userProfilLocalPath))
                _fileManager.CreateDirectory(_userProfilLocalPath);
        }

        protected override async void Show()
        {
            base.Show();

            SynchronizeData();

            await ManageUserConnection();

            await SynchronizeWebData();
        }

		private void SynchronizeData()
		{
			TitleLabel = "BodyReport";
			MenuLabel = Translation.Get (TRS.MENU);
			ConfigurationLabel = Translation.Get (TRS.CONFIGURATION);
			TrainingJournalLabel = Translation.Get (TRS.TRAINING_JOURNAL);
			ChangeLanguageLabel = Translation.Get (TRS.LANGUAGE);
			LanguageFlagImageSource = GeLanguageFlagImageSource (Translation.CurrentLang);

            OnPropertyChanged(null);
        }

		private string GeLanguageFlagImageSource(LangType langType)
		{
			return string.Format ("flag_{0}.png", Translation.GetLangExt (langType)).Replace('-', '_');
		}

        private string GetUserImageLocalPath()
        {
            if (string.IsNullOrWhiteSpace(UserData.Instance.UserInfo.UserId))
                return null;
            return Path.Combine(_userProfilLocalPath, UserData.Instance.UserInfo.UserId + ".png");
        }

        private void DisplayUserProfil()
        {
            UserProfilImage = GetUserImageLocalPath();
        }

        private async Task ManageUserConnection()
        {
            ActionIsInProgress = true;

            try
            {
                if (LoginManager.Instance.Init())
                {
                    DisplayUserProfil();
                    await LoginManager.Instance.ConnectUser(false); // no need treat response, just for connect user
                }
                else
                {
                    await LoginViewModel.DisplayViewModel();
                }
            }
            catch //(Exception except)
            {
            }
            finally
            {
                ActionIsInProgress = false;
            }
        }

        private async Task SynchronizeWebData()
		{
            ActionIsInProgress = true;
            try
			{
                // download user image
                string localUserImagePath = GetUserImageLocalPath();
                string urlImage = string.Format("{0}images/userprofil/{1}.png", HttpConnector.Instance.BaseUrl, UserData.Instance.UserInfo.UserId);
                if (await HttpConnector.Instance.DownloadFile(urlImage, GetUserImageLocalPath()))
                    DisplayUserProfil();
                
                //Synchronise Web data to local database
                var muscleList = await MuscleWebService.FindMuscles();
				var muscleManager = new MuscleManager(_dbContext);
				muscleManager.UpdateMuscleList(muscleList);

				var translationList = await TranslationWebService.FindTranslations();
				var translationManager = new TranslationManager(_dbContext);
				translationManager.UpdateTranslationList(translationList);
                
			}
			catch (Exception exception)
			{
				// TODO log
			}
            finally
            {
                ActionIsInProgress = false;
            }
		}

		public ICommand GoToTrainingJournalCommand
		{
			get
			{
				return new Command (async () => {
                    var viewModel = new TrainingJournalViewModel();
                    await ShowModalViewModel(viewModel, this);
				});
			}
		}

        /// <summary>
        /// Change language with user choice list view
        /// </summary>
		public ICommand GoToChangeLanguageCommand
		{
			get
			{
				return new Command(async () => {

					var datas = new List<GenericData> ();

					string trName;
					var languageValues = Enum.GetValues(typeof(LangType));
					GenericData data, currentData = null;
					foreach(LangType languageValue in languageValues)
					{
						trName = languageValue == LangType.en_US ? "English" : "Français";
						data = new GenericData(){ Tag = languageValue, Name = trName, Image = GeLanguageFlagImageSource(languageValue)};
						datas.Add(data);

						if(languageValue == Translation.CurrentLang)
							currentData = data;
					}

					var result = await ListViewModel.ShowGenericList (Translation.Get(TRS.LANGUAGE), datas, currentData, this);

					if(result.Validated && result.SelectedData != null && result.SelectedData.Tag != null)
					{
						Translation.ChangeLang((LangType)result.SelectedData.Tag);
						SynchronizeData();
					}

				});
			}
		}
	}
}


