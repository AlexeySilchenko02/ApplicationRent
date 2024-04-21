﻿using ApplicationRent.Data.Identity;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;

namespace ApplicationRent.App_data
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebase;

        public FirebaseService()
        {
            // Учетные данные Google Cloud из файла JSON
            var credential = GoogleCredential.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "rent.json"))
                              .CreateScoped("https://www.googleapis.com/auth/firebase.database",
                                            "https://www.googleapis.com/auth/userinfo.email");

            // Инициализация клиента Firebase
            _firebase = new FirebaseClient(
                "https://rent-c4c65-default-rtdb.europe-west1.firebasedatabase.app/", //URL Firebase Realtime Database
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => credential.UnderlyingCredential.GetAccessTokenForRequestAsync()
                });
        }

        public async Task AddOrUpdatePlace(Place place)
        {
            // Добавление или обновление данных о месте в Firebase
            await _firebase
                .Child("Places")
                .Child(place.Id.ToString())
                .PutAsync(place);
        }

        public async Task DeletePlace(int id)
        {
            // Удаление данных о месте из Firebase
            await _firebase
                .Child("Places")
                .Child(id.ToString())
                .DeleteAsync();
        }

        public async Task AddOrUpdateRental(Rental rental)
        {
            // Добавление или обновление данных об аренде в Firebase
            await _firebase
                .Child("Rentals")
                .Child(rental.Id.ToString())
                .PutAsync(rental);
        }
    }
}
