create table person (
	id int primary key autoincrement,
	employerId int references employer (id),
	name string(100)
);