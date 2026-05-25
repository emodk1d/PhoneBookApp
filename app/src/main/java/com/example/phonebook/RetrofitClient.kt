package com.example.phonebook

import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import kotlin.jvm.java

object RetrofitClient {
    private const val BASE_URL = "http://192.168.88.178:5057/"

    val api: PhoneBookAPI by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(PhoneBookAPI::class.java)
    }
}