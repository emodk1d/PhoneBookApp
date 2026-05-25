package com.example.phonebook

data class Contact(
    val id: Int,
    val lastName: String,
    val firstName: String,
    val middleName: String?,
    val fullName: String,
    val phones: List<Phone>
)