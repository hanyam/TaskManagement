select * from Tasks.Users u where u.Email like'%hmohamed%';

select * from Tasks.Users u where u.Email like'%sbalateef@wtco.com.sa%';


select * from Tasks.ManagerEmployees me where me.ManagerId ='AEF17F56-FCEB-4BA3-9129-8101549D40E9';

select *
--update  
--from
Tasks.ManagerEmployees 
set ManagerId='F8B7E301-D100-4946-B042-6E3B29417748' 
where ManagerId ='AEF17F56-FCEB-4BA3-9129-8101549D40E9' and EmployeeId <> 'F8B7E301-D100-4946-B042-6E3B29417748' ;