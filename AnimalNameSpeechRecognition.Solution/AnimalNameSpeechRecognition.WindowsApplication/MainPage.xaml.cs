using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace AnimalNameSpeechRecognition.WindowsApplication
{
    public sealed partial class MainPage
    {
        SpeechRecognizer _speechRecognizer;
        private CoreDispatcher _dispatcher;

        public MainPage()
        {
            InitializeComponent();
        }

        public async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            var permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (!permissionGained) return;

            await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
        }

        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (_speechRecognizer != null)
            {
                _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                _speechRecognizer.Dispose();
                _speechRecognizer = null;
            }

            _speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            var expectedResponses = GetAnimalList();
            var listConstraint = new SpeechRecognitionListConstraint(expectedResponses, "Animals");
            _speechRecognizer.Constraints.Add(listConstraint);
            await _speechRecognizer.CompileConstraintsAsync();

            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await ShowAnimal(args.Result.Text);
                });
            }
        }

        private Task ShowAnimal(string phrase)
        {
            var cleanedPhrase = phrase.ToLower().Replace(".", string.Empty);
            var animals = GetAnimalList();

            if (animals.Contains(cleanedPhrase))
            {
                AnimalImage.Source = new BitmapImage(new Uri($"ms-appx:///Images/Animals/{cleanedPhrase}.jpg"));
            }
            
            AnimalName.Text = cleanedPhrase;

            return Task.CompletedTask;
        }

        private static string[] GetAnimalList()
        {
            return new[] {
                "aardvark",
                "badger",
                "dolphin",
                "duck",
                "fox",
                "guinea pig",
                "hamster",
                "kangaroo",
                "meerkat",
                "mouse",
                "owl",
                "panda",
                "pig",
                "monkey",
                "elephant",
                "rhino",
                "giraffe",
                "penguin",
                "lion",
                "tiger",
                "snake",
                "fish",
                "dog",
                "cat",
                "rabbit",
                "bear",
                "snake",
                "frog",
                "lizard",
                "tortoise",
                "cow",
                "goat",
                "hippo",
                "horse",
                "sheep",
                "zebra",
                "donkey",
                "bird",
                "whale",
            };
        }
    }
}
