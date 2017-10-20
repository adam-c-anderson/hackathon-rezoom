create table pet (
	id int primary key autoincrement,
	personId int references person (id),
	name string
);