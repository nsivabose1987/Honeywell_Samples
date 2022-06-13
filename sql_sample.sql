/*
- merge statement will be used in case of continuous update of existing table using a batch process.
- target is the main table where the data needs to refreshed
- source is the interim staging table which has the latest data recived from upstream
- "when matched" when records are matched, we update the records in target table as per source table
- "when not matched by target" when source records are not found in target, we do the inserts 
- "when not matched by source" when target records are not found in source, we do the delete. 
- I also used the results from output cluase to table variable to get the number of insert/update/delete (s) made
*/

declare @resulttable table (action_verb varchar(10))
merge ad_users as target
using ad_users_load as source
on target.user_id = source.user_id
WHEN MATCHED THEN
update set user_name = source.user_name,
	manager_user_id = source.manager_user_id
when not matched by target then
insert (user_id, user_name, manager_user_id)
values (source.user_id, source.user_name, source.manager_user_id)
when not matched by source then
delete
output $action into @resulttable;
select action_verb, count(*) from @resulttable
group by action_verb


/*
- recursive CTES are used to get parent-chidl relationship.
*/

;With XE AS (
	select 
		user_id,
		user_name,
		manager_user_id
	from ad_users WITH (nolock)
	where manager_user_id is not null
	UNION ALL
	select 
		ad_users.user_id,
		ad_users.user_name,
		ad_users.manager_user_id
	from ad_users WITH (nolock)
	JOIN XE
		on xe.user_id = ad_users.manager_user_id
)
select * from XE 
order by 3