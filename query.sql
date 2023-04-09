create database Todo;

use Todo;

create table Users (
	Id int identity(1,1) primary key,
	Email varchar(200) not null,
	Password varchar(200) not null
);

create table Items (
	Id int identity(1,1) primary key,
	Name varchar(100) not null,
	Description varchar(200) not null,
	CreatedDate datetime2 not null,
	UpdatedDate datetime2,
	IsCompleted bit not null
);

insert into Users values ('teste@teste.com', '123456');