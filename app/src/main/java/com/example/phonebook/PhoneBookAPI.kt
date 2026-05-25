package com.example.phonebook

import retrofit2.http.GET

interface PhoneBookAPI {
    @GET("api/contacts")
    suspend fun getContacts(): List<Contact>
}