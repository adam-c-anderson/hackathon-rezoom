create table person (
	id int primary key autoincrement,
	name string(100)
);

create table pet (
	id int primary key autoincrement,
	personId int references person (id),
	name string
);