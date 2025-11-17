create table #TheTable(ManagerEmail varchar(500),EmployeeEmail varchar(500));


select * from TaskManagement.Tasks.ManagerEmployees;

declare @dt datetime = getdate();
insert into TaskManagement.Tasks.ManagerEmployees
select Newid(),
	--t.ManagerEmail,
	u.Id,
	--t.EmployeeEmail,
	u2.Id,
	 @dt,
	 NULL,
	 'System',
	 NULL
from
	#TheTable as t
join Tasks.Users u on
	u.Email = t.ManagerEmail
join Tasks.Users u2 on
	u2.Email = t.EmployeeEmail
order by
	1;


	INSERT INTO #TheTable VALUES
	 (N'aalkhalaf@wtco.com.sa',N'aalmusaad@wtco.com.sa'),
	 (N'aalkhalaf@wtco.com.sa',N'rmalmutairi@wtco.com.sa'),
	 (N'aalkhalaf@wtco.com.sa',N'dalqabbani@wtco.com.sa'),
	 (N'aalkhalaf@wtco.com.sa',N'malasiri@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'ealdhowaihi@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'wabubtain@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'malshalan@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'aalrasheed@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'raldossari@wtco.com.sa'),
	 (N'halmajed@wtco.com.sa',N'maldosari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'halmajed@wtco.com.sa',N'aaalasmari@wtco.com.sa'),
	 (N'aalsedairy@wtco.com.sa',N'anegm@wtco.com.sa'),
	 (N'aalsedairy@wtco.com.sa',N'sbalateef@wtco.com.sa'),
	 (N'aalsedairy@wtco.com.sa',N'halmajed@wtco.com.sa'),
	 (N'aalsedairy@wtco.com.sa',N'mmohey@wtco.com.sa'),
	 (N'aalhumaydhi@wtco.com.sa',N'falosaimi@wtco.com.sa'),
	 (N'aalhumaydhi@wtco.com.sa',N'balanizi@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'afaldawood@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'gfagerah@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'kaljabr@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mmohey@wtco.com.sa',N'afalshammari@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'malahyani@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'salthobaiti@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'aalghubayni@wtco.com.sa'),
	 (N'mmohey@wtco.com.sa',N'rmalotaibi@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'amalsultan@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'aalmansour@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'ahaalghamdi@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'aalghanim@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'aalmaymouni@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mmubarak@wtco.com.sa',N'malrushaidi@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'salfadhl@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'saltuwayjiri@wtco.com.sa'),
	 (N'mmubarak@wtco.com.sa',N'asalshahrani@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalmuhmmdi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'ssaljohani@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'smoalharbi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'amalsubhi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'amalsobhi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'halshareef@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalsiary@wtco.com.sa',N'malgahni@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalfredi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'atalsobhi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'smualharbi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aoalahmadi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'ahalyoubi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalsubhi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalmohammadi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalnakhli@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'HAlrashidi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalsiary@wtco.com.sa',N'ialmohammadi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'ralanazi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'hsaljohani@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'nberekit@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'shalharbi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'abahowaini@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'amalsulami@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'hmasiri@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'ralyoubi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'imuhllwi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalsiary@wtco.com.sa',N'hraljohani@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'malmeqaty@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'faalhazmi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aalmabadi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'amaaljohani@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'aralsubhi@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'malhamed@wtco.com.sa'),
	 (N'kalsiary@wtco.com.sa',N'halmowalld@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'mmalahmari@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'analharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aallehiani@wtco.com.sa',N'oalawfi@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'halyoubi@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'malqhtani@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'halsehli@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'aalhulaiw@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'omalharbi@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'naljohani@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'ralshareef@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'zalmusawa@wtco.com.sa'),
	 (N'aallehiani@wtco.com.sa',N'farisalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aallehiani@wtco.com.sa',N'falmokhlafi@wtco.com.sa'),
	 (N'aalhazmi@wtco.com.sa',N'salmehmadi@wtco.com.sa'),
	 (N'aalhazmi@wtco.com.sa',N'fbadughaish@wtco.com.sa'),
	 (N'aalhazmi@wtco.com.sa',N'amalnashri@wtco.com.sa'),
	 (N'nalghamdi@wtco.com.sa',N'malyahyawi@wtco.com.sa'),
	 (N'nalghamdi@wtco.com.sa',N'talbaqami@wtco.com.sa'),
	 (N'nalghamdi@wtco.com.sa',N'salmazarkah@wtco.com.sa'),
	 (N'salmehmadi@wtco.com.sa',N'akilani@wtco.com.sa'),
	 (N'salmehmadi@wtco.com.sa',N'fallohybi@wtco.com.sa'),
	 (N'salmehmadi@wtco.com.sa',N'HAALHARBI@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salmehmadi@wtco.com.sa',N'iialharbi@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'hmadkhali@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'sfalghamdi@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'admobarki@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'mgharwa@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'ayalharbi@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'ahmadasiri@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'aalswaid@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'mmoti@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'amohammed@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'tasseri@wtco.com.sa',N'mettwadi@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'mayalgarni@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'amobarki@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'srasiri@wtco.com.sa'),
	 (N'tasseri@wtco.com.sa',N'kabuallut@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'zaseri@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'aalayyafi@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'ealfaifi@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'aabakri@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'eshaqiqi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'atawhari@wtco.com.sa',N'maljazzam@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'halshahrani@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'yamri@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'aahasiri@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'ashajry@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'mfadhli@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'hasiri@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'nakhterbutt@wtco.com.sa'),
	 (N'atawhari@wtco.com.sa',N'aalrajhi@wtco.com.sa'),
	 (N'aalfaifi@wtco.com.sa',N'kaalanazi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalfaifi@wtco.com.sa',N'aahassan@wtco.com.sa'),
	 (N'aalfaifi@wtco.com.sa',N'malshaikh@wtco.com.sa'),
	 (N'aalfaifi@wtco.com.sa',N'balsanea@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'aalhazmi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'nalghamdi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'aalfaifi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'kalsafi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'wdabbour@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'aalroshoud@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'aalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'arahbini@wtco.com.sa',N'kaalshammari@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'dhasousah@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'talmohammadi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'yalqurashi@wtco.com.sa'),
	 (N'arahbini@wtco.com.sa',N'TAlMunayir@wtco.com.sa'),
	 (N'kalsafi@wtco.com.sa',N'tasseri@wtco.com.sa'),
	 (N'kalsafi@wtco.com.sa',N'atawhari@wtco.com.sa'),
	 (N'kalsafi@wtco.com.sa',N'iageli@wtco.com.sa'),
	 (N'kalsafi@wtco.com.sa',N'ialfaifi@wtco.com.sa'),
	 (N'kalsafi@wtco.com.sa',N'aasiry@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalsafi@wtco.com.sa',N'aalnami@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'kalsiary@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'aallehiani@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'aalkhathami@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'marzouqalharbi@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'yalsayed@wtco.com.sa'),
	 (N'wdabbour@wtco.com.sa',N'nzalharbi@wtco.com.sa'),
	 (N'malasiri@wtco.com.sa',N'ahalthagafi@wtco.com.sa'),
	 (N'malasiri@wtco.com.sa',N'mmalmutairi@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'aalomari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'talnaeem@wtco.com.sa',N'tkachouri@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'jalzahrani@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'aayaldawsari@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'talfaris@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'aalhumaydhi@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'wnaimi@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'kalhabib@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'ahassan@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'aalhejji@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'arahbini@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'talnaeem@wtco.com.sa',N'smalqahtani@wtco.com.sa'),
	 (N'talnaeem@wtco.com.sa',N'aalabdulkader@wtco.com.sa'),
	 (N'ahassan@wtco.com.sa',N'aalshalfan@wtco.com.sa'),
	 (N'ahassan@wtco.com.sa',N'salshehri@wtco.com.sa'),
	 (N'ahassan@wtco.com.sa',N'aaalhussain@wtco.com.sa'),
	 (N'ahassan@wtco.com.sa',N'kalsallum@wtco.com.sa'),
	 (N'ahassan@wtco.com.sa',N'rjaiswal@wtco.com.sa'),
	 (N'aalhejji@wtco.com.sa',N'maljarallah@wtco.com.sa'),
	 (N'aalhejji@wtco.com.sa',N'kaljandal@wtco.com.sa'),
	 (N'aalhejji@wtco.com.sa',N'amalzahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalhejji@wtco.com.sa',N'salsadoon@wtco.com.sa'),
	 (N'aalhejji@wtco.com.sa',N'aalshammary@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'aalsedairy@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'malbilbisi@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'aalwabel@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'kaalshibl@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'aalhalwan@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'fbinrubayyi@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'aalmubrad@wtco.com.sa'),
	 (N'aalabdulkader@wtco.com.sa',N'talmuzaini@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalabdulkader@wtco.com.sa',N'aalquraish@wtco.com.sa'),
	 (N'salsadoon@wtco.com.sa',N'NALSUMRANI@wtco.com.sa'),
	 (N'salsadoon@wtco.com.sa',N'rghashyan@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'saloraimah@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'aalzhrani@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'malbuainain@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'malfahid@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'mdalmutairi@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'aaldawser@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'aalrashdi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'nalhumaidi@wtco.com.sa',N'majedalqahtani@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'abdullahmoalharbi@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'aalmatar@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'ealgharash@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'jcuico@wtco.com.sa'),
	 (N'nalhumaidi@wtco.com.sa',N'mutabalanzi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'aalfahid@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'mmualmutairi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'salbuainain@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'ahudaib@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'maseri@wtco.com.sa',N'malmuziny@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'nalajmi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'yalanazi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'snalqahtani@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'nalqatani@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'halsohli@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'faalmutairi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'nalzahrani@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'saadalharbi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'fkalshammari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'maseri@wtco.com.sa',N'hfalshammari@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'kalajmi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'maalghamdi@wtco.com.sa'),
	 (N'maseri@wtco.com.sa',N'aaasiry@wtco.com.sa'),
	 (N'raldakhil@wtco.com.sa',N'mnkhan@wtco.com.sa'),
	 (N'raldakhil@wtco.com.sa',N'wbajaba@wtco.com.sa'),
	 (N'raldakhil@wtco.com.sa',N'hsulaiman@wtco.com.sa'),
	 (N'aalharbi@wtco.com.sa',N'asuhil@wtco.com.sa'),
	 (N'aalharbi@wtco.com.sa',N'aalnafisah@wtco.com.sa'),
	 (N'aalshammary@wtco.com.sa',N'foalshahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalshammary@wtco.com.sa',N'aaleidi@wtco.com.sa'),
	 (N'aalshammary@wtco.com.sa',N'amalsaleh@wtco.com.sa'),
	 (N'aalshammary@wtco.com.sa',N'haalsharif@wtco.com.sa'),
	 (N'smalqahtani@wtco.com.sa',N'abeeralghamdi@wtco.com.sa'),
	 (N'smalqahtani@wtco.com.sa',N'aalkhalaf@wtco.com.sa'),
	 (N'smalqahtani@wtco.com.sa',N'mmubarak@wtco.com.sa'),
	 (N'smalqahtani@wtco.com.sa',N'walkhanfari@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'aalghamdi@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'aalsalem@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'aalmarshad@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalshuaibi@wtco.com.sa',N'aalbishri@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'jsenin@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'manwar@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'SSurendran@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'MTaha@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'sdaimi@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'shusssan@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'SAmin@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'MNasr@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'MAlBadry@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalshuaibi@wtco.com.sa',N'aaladwani@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'ANazir@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'aalhassani@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'wahmad@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'MIqbal@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'npanadit@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'MAbdulgafar@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'kalharbi@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'tfatani@wtco.com.sa'),
	 (N'aalshuaibi@wtco.com.sa',N'salshammari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalshuaibi@wtco.com.sa',N'kalnefaie@wtco.com.sa'),
	 (N'salshehri@wtco.com.sa',N'nalobaid@wtco.com.sa'),
	 (N'salshehri@wtco.com.sa',N'amallik@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'bfalanazi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'rsalzahrani@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'nalashqan@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'salabdullah@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'malbahussain@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'saalharbi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'salmuwayni@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ialsenani@wtco.com.sa',N'nalsheddi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'aalsaleh@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'farrajalharbi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'snoalharbi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'aalhussain@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'aalghazi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'falshammary@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'afalsaeed@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'asalotaibi@wtco.com.sa'),
	 (N'ialsenani@wtco.com.sa',N'mmaalghamdi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malmilhim@wtco.com.sa',N'halmojeel@wtco.com.sa'),
	 (N'malmilhim@wtco.com.sa',N'malabdulmohsen@wtco.com.sa'),
	 (N'malmilhim@wtco.com.sa',N'jalabdulaziz@wtco.com.sa'),
	 (N'malmilhim@wtco.com.sa',N'awalghamdi@wtco.com.sa'),
	 (N'malmilhim@wtco.com.sa',N'salqarni@wtco.com.sa'),
	 (N'mjawad@wtco.com.sa',N'ralmuhawes@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'ysalghamdi@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'salbeshri@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'ahaijan@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'oalnajm@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amalzahrani@wtco.com.sa',N'mbinakail@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'aalshuaibi@wtco.com.sa'),
	 (N'amalzahrani@wtco.com.sa',N'malgwaizani@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'ralghamri@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'adawarihas@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'ealfouzan@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'ialnemer@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'nalhussain@wtco.com.sa'),
	 (N'walkhanfari@wtco.com.sa',N'nalfhaid@wtco.com.sa'),
	 (N'aalshalfan@wtco.com.sa',N'nalrasheedi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mbinakail@wtco.com.sa',N'falturki@wtco.com.sa'),
	 (N'mbinakail@wtco.com.sa',N'aalmutarrid@wtco.com.sa'),
	 (N'aalsalem@wtco.com.sa',N'MBAlmutairi@wtco.com.sa'),
	 (N'aalrusayyis@wtco.com.sa',N'yalshehri@wtco.com.sa'),
	 (N'aalrusayyis@wtco.com.sa',N'kaljabri@wtco.com.sa'),
	 (N'aalrusayyis@wtco.com.sa',N'azakri@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'ssamhan@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'maljubir@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'talmojel@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'maalqahtani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malbilbisi@wtco.com.sa',N'aalthonayan@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'malobaid@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'msalshaikhi@wtco.com.sa'),
	 (N'malbilbisi@wtco.com.sa',N'ralghamdi@wtco.com.sa'),
	 (N'malbuainain@wtco.com.sa',N'walfarraj@wtco.com.sa'),
	 (N'malbuainain@wtco.com.sa',N'fhethleen@wtco.com.sa'),
	 (N'malbuainain@wtco.com.sa',N'halajmi@wtco.com.sa'),
	 (N'malbuainain@wtco.com.sa',N'fmualmutairi@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'aaljobrah@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'saldawsari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'falhajri@wtco.com.sa',N'falhushum@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'salamri@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'ialfayhan@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'haljazeri@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'aaldakhil@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'aaalmarri@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'fhathlin@wtco.com.sa'),
	 (N'falhajri@wtco.com.sa',N'aalsayhah@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'aalalshammari@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'aalbathali@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mdalmutairi@wtco.com.sa',N'amalanzi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'analmarri@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'aralajmi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'msalmutairi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'nfalharbi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'mnalmutairi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'falshammari@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'ialenzi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'kmalotaibi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'malmarri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mdalmutairi@wtco.com.sa',N'amalshammari@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'menwralanzi@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'salsuhali@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'aalabdan@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'jalbaiji@wtco.com.sa'),
	 (N'mdalmutairi@wtco.com.sa',N'hhalshammari@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'mralshammari@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'amualmutairi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'amialmutairi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'omalmutairi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malfahid@wtco.com.sa',N'mmealmutairi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'mhaalqahtani@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'saalanazi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'muhannaalshammari@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'ealenezi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'fmoalmutairi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'fmaalanazi@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'malnaseeb@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'aaaldawsari@wtco.com.sa'),
	 (N'malfahid@wtco.com.sa',N'aalsaqer@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'talbuqami@wtco.com.sa',N'maaldossary@wtco.com.sa'),
	 (N'talbuqami@wtco.com.sa',N'aalrumi@wtco.com.sa'),
	 (N'talbuqami@wtco.com.sa',N'balrehaili@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'raldawai@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'mmoalghamdi@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'aalsuwaidan@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'salqallaf@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'malnafiee@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'talbuqami@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'falhajri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'dhasousah@wtco.com.sa',N'nalhumaidi@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'maseri@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'nalkahtani@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'mhualqahtani@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'malsakran@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'smoalmarri@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'mohannadalghamdi@wtco.com.sa'),
	 (N'dhasousah@wtco.com.sa',N'mzaalqahtani@wtco.com.sa'),
	 (N'malnafiee@wtco.com.sa',N'walharithi@wtco.com.sa'),
	 (N'malnafiee@wtco.com.sa',N'badralmutairi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malnafiee@wtco.com.sa',N'falruwaili@wtco.com.sa'),
	 (N'malnafiee@wtco.com.sa',N'mubarakalshammari@wtco.com.sa'),
	 (N'malnafiee@wtco.com.sa',N'falarafah@wtco.com.sa'),
	 (N'malnafiee@wtco.com.sa',N'falsawillh@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'aalabullah@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'aalmotairi@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'fhalshammari@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'aabalshammari@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'ahalanazi@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'zsalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malahmed@wtco.com.sa',N'kalanazi@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'halsalis@wtco.com.sa'),
	 (N'malahmed@wtco.com.sa',N'fbaalmutairi@wtco.com.sa'),
	 (N'aalsuwaidan@wtco.com.sa',N'aalenazi@wtco.com.sa'),
	 (N'salqallaf@wtco.com.sa',N'haljbaili@wtco.com.sa'),
	 (N'salqallaf@wtco.com.sa',N'fgalshammari@wtco.com.sa'),
	 (N'salqallaf@wtco.com.sa',N'ealqawo@wtco.com.sa'),
	 (N'salqallaf@wtco.com.sa',N'fmualanazi@wtco.com.sa'),
	 (N'aalrashdi@wtco.com.sa',N'maqalharbi@wtco.com.sa'),
	 (N'aalrashdi@wtco.com.sa',N'naldousari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalrashdi@wtco.com.sa',N'nalbaqami@wtco.com.sa'),
	 (N'aalrashdi@wtco.com.sa',N'aalasqah@wtco.com.sa'),
	 (N'aalrashdi@wtco.com.sa',N'talmuhini@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'halshmmeri@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'aalenezi@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'aalmutiri@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'nalshaibani@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'ktalharbi@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'malmqati@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'galherbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'majedalqahtani@wtco.com.sa',N'malsayg@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'salasmari@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'falqashi@wtco.com.sa'),
	 (N'majedalqahtani@wtco.com.sa',N'salanzy@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'mibntuwalah@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'mabalmutairi@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'mmualshammari@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'ymalharbi@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'ahalshammari@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'nalkhaldy@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'fgalshammari@wtco.com.sa',N'agalshammari@wtco.com.sa'),
	 (N'fgalshammari@wtco.com.sa',N'bsalmutairi@wtco.com.sa'),
	 (N'nalajmi@wtco.com.sa',N'hhalmalki@wtco.com.sa'),
	 (N'nalajmi@wtco.com.sa',N'fahadbaalharbi@wtco.com.sa'),
	 (N'haljbaili@wtco.com.sa',N'salanzi@wtco.com.sa'),
	 (N'haljbaili@wtco.com.sa',N'ealanazi@wtco.com.sa'),
	 (N'aalenazi@wtco.com.sa',N'amalkhaldi@wtco.com.sa'),
	 (N'aalenazi@wtco.com.sa',N'manifalshammari@wtco.com.sa'),
	 (N'aalenazi@wtco.com.sa',N'muathsalghamdi@wtco.com.sa'),
	 (N'aalenazi@wtco.com.sa',N'talyami@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalenazi@wtco.com.sa',N'falghamdi@wtco.com.sa'),
	 (N'aalenazi@wtco.com.sa',N'ialhusayni@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'aalalhazmi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'bandaralmutairi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'nalkathiri@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'amualharbi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'aaalfifi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'meshalalharbi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'salnajashi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'sdalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malsakran@wtco.com.sa',N'aalduweirej@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'ialmutairi@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'aalhaiz@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'mohammedalshammari@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'falkwyben@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'aalserhan@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'falsubaie@wtco.com.sa'),
	 (N'malsakran@wtco.com.sa',N'asalanazi@wtco.com.sa'),
	 (N'mohannadalghamdi@wtco.com.sa',N'araldossari@wtco.com.sa'),
	 (N'mohannadalghamdi@wtco.com.sa',N'jalhelal@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mohannadalghamdi@wtco.com.sa',N'ralhushim@wtco.com.sa'),
	 (N'mohannadalghamdi@wtco.com.sa',N'malahmed@wtco.com.sa'),
	 (N'mohannadalghamdi@wtco.com.sa',N'aalwaeed@wtco.com.sa'),
	 (N'ahalluhaybi@wtco.com.sa',N'ialhazmi@wtco.com.sa'),
	 (N'yalsayed@wtco.com.sa',N'aalmubarak@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'aalrozini@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'balhammad@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'malsulami@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'halenazi@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'ajalalwani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'nzalharbi@wtco.com.sa',N'halbeati@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'falsubhi@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'aaljohani@wtco.com.sa'),
	 (N'nzalharbi@wtco.com.sa',N'fsaljohani@wtco.com.sa'),
	 (N'halenazi@wtco.com.sa',N'meid@wtco.com.sa'),
	 (N'aabdullah@wtco.com.sa',N'maltuwayjiri@wtco.com.sa'),
	 (N'aabdullah@wtco.com.sa',N'mamalotaibi@wtco.com.sa'),
	 (N'aabdullah@wtco.com.sa',N'iabahri@wtco.com.sa'),
	 (N'aalmuhmmdi@wtco.com.sa',N'aalalyoubi@wtco.com.sa'),
	 (N'aalmuhmmdi@wtco.com.sa',N'amathhar@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmuhmmdi@wtco.com.sa',N'walhojili@wtco.com.sa'),
	 (N'aalmuhmmdi@wtco.com.sa',N'mbogis@wtco.com.sa'),
	 (N'aalmuhmmdi@wtco.com.sa',N'aabdulhalim@wtco.com.sa'),
	 (N'aalmuhmmdi@wtco.com.sa',N'haldeeb@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'aalarawi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'ymasoudi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'fmalharbi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'malohibi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'ralhelali@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'asalkhalaf@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'smoalharbi@wtco.com.sa',N'aoalansari@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'mhuraysi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'mmaalharbi@wtco.com.sa'),
	 (N'smoalharbi@wtco.com.sa',N'ijaber@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'yalshaiban@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'mhamde@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'mabdulrzaq@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'falrehele@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'aalalwe@wtco.com.sa'),
	 (N'haloufi@wtco.com.sa',N'salblowi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amralnakhli@wtco.com.sa',N'talsobhi@wtco.com.sa'),
	 (N'amralnakhli@wtco.com.sa',N'salrefaei@wtco.com.sa'),
	 (N'amralnakhli@wtco.com.sa',N'talalwni@wtco.com.sa'),
	 (N'naljohani@wtco.com.sa',N'haloufi@wtco.com.sa'),
	 (N'naljohani@wtco.com.sa',N'amralnakhli@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'aalmelabi@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'ealmilabi@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'asalluhaybi@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'oqabalharbi@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'mialhazmi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaljohani@wtco.com.sa',N'ralturki@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'talmuhamadi@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'halshinqeti@wtco.com.sa'),
	 (N'kaljohani@wtco.com.sa',N'halomie@wtco.com.sa'),
	 (N'ralshareef@wtco.com.sa',N'balissa@wtco.com.sa'),
	 (N'ralshareef@wtco.com.sa',N'bbraikeet@wtco.com.sa'),
	 (N'aaljazaeri@wtco.com.sa',N'halmehmadi@wtco.com.sa'),
	 (N'aaljazaeri@wtco.com.sa',N'ealsobhi@wtco.com.sa'),
	 (N'aaljazaeri@wtco.com.sa',N'yousefalsubhi@wtco.com.sa'),
	 (N'aaljazaeri@wtco.com.sa',N'salsubhi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aaljazaeri@wtco.com.sa',N'bmhawsawi@wtco.com.sa'),
	 (N'aaljazaeri@wtco.com.sa',N'sultanalharbi@wtco.com.sa'),
	 (N'jalmuwallad@wtco.com.sa',N'mfaalsubhi@wtco.com.sa'),
	 (N'farisalharbi@wtco.com.sa',N'yalzaylaee@wtco.com.sa'),
	 (N'farisalharbi@wtco.com.sa',N'msalshehri@wtco.com.sa'),
	 (N'farisalharbi@wtco.com.sa',N'aabdullah@wtco.com.sa'),
	 (N'farisalharbi@wtco.com.sa',N'mmaljohani@wtco.com.sa'),
	 (N'farisalharbi@wtco.com.sa',N'raalharbi@wtco.com.sa'),
	 (N'falmokhlafi@wtco.com.sa',N'yalhossen@wtco.com.sa'),
	 (N'yalzaylaee@wtco.com.sa',N'msalsubhi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'yalzaylaee@wtco.com.sa',N'ysalsubhi@wtco.com.sa'),
	 (N'yalzaylaee@wtco.com.sa',N'maloufi@wtco.com.sa'),
	 (N'yalzaylaee@wtco.com.sa',N'malsharari@wtco.com.sa'),
	 (N'yalzaylaee@wtco.com.sa',N'malhamdan@wtco.com.sa'),
	 (N'falsubhi@wtco.com.sa',N'galhazmi@wtco.com.sa'),
	 (N'falsubhi@wtco.com.sa',N'ahalluhaybi@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'walsobhy@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'ssaalzahrani@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'malrajabi@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'yyahyaoui@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'bbraikeet@wtco.com.sa',N'akaabi@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'malzubaidi@wtco.com.sa'),
	 (N'bbraikeet@wtco.com.sa',N'adalsobhi@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'aaljadaani@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'aalmailm@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'smoutwkal@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'ehalawani@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'balmistadi@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'aalhmda@wtco.com.sa'),
	 (N'msalshehri@wtco.com.sa',N'malmistadi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'omalharbi@wtco.com.sa',N'hwalharbi@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'aaljazaeri@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'aaalsobhi@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'baalharbi@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'halolowi@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'isomily@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'ealbajhan@wtco.com.sa'),
	 (N'omalharbi@wtco.com.sa',N'obarasheed@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'falahmadi@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'hsalmalki@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'balissa@wtco.com.sa',N'alagby@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'aqahtani@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'aoalsubhi@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'mmohammed@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'galmukhlifi@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'jalmuwallad@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'hbahakim@wtco.com.sa'),
	 (N'balissa@wtco.com.sa',N'yabuhawa@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'shaljohani@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'kaljohani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalfredi@wtco.com.sa',N'yasseralsubhi@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'falmohammadi@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'malhejely@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'yatiahallah@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'aaealharbi@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'aramadan@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'oalahmadi@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'zatalharbi@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'abeek@wtco.com.sa'),
	 (N'aalfredi@wtco.com.sa',N'malsabhi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aoalahmadi@wtco.com.sa',N'aalqrni@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'akherisa@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'waljohani@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'faisalalmutairi@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'bmalharbi@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'amaljohani@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'bdalmutairi@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'sattamalharbi@wtco.com.sa'),
	 (N'aoalahmadi@wtco.com.sa',N'ayalahmadi@wtco.com.sa'),
	 (N'ahalyoubi@wtco.com.sa',N'malsahali@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ahalyoubi@wtco.com.sa',N'ahalansari@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'abawazir@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'abdullahalalshehri@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'halhrbi@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'malamir@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'kaljarah@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'nabdulaal@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'walshehri@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'zqashgari@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'oaljardahi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malsahali@wtco.com.sa',N'balsubhi@wtco.com.sa'),
	 (N'malsahali@wtco.com.sa',N'sjalmalki@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'msaalsharif@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'mshibayn@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'halhajouj@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'nalsemairi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'abalanazi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'halthagafi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'abdullahaljohani@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'hwaznah@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ahalansari@wtco.com.sa',N'oasiri@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'malgehani@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'afaqihi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'falhardi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'abdullahmoalghamdi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'aealalwani@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'aalhawsa@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'haljawi@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'kalnafei@wtco.com.sa'),
	 (N'ahalansari@wtco.com.sa',N'nalhafezi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ahalansari@wtco.com.sa',N'ralamri@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'mhaljumah@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'ybafarhan@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'snalghamdi@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'aaldokhi@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'ffelemban@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'aaloshbah@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'halabbadi@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'falhussain@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'maldossary@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalbakheet@wtco.com.sa',N'malablan@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'aalmuhaisin@wtco.com.sa'),
	 (N'kalbakheet@wtco.com.sa',N'aalomar@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'oalawad@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'talthawadi@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'falhabub@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'jaldossriy@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'kaldossari@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'kalabbad@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'falsubaei@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salabood@wtco.com.sa',N'halameri@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'kaleid@wtco.com.sa'),
	 (N'salabood@wtco.com.sa',N'aalsehali@wtco.com.sa'),
	 (N'ialqooba@wtco.com.sa',N'jalleef@wtco.com.sa'),
	 (N'ialqooba@wtco.com.sa',N'mabdulkarem@wtco.com.sa'),
	 (N'ialqooba@wtco.com.sa',N'malnayf@wtco.com.sa'),
	 (N'ialqooba@wtco.com.sa',N'aalnajrani@wtco.com.sa'),
	 (N'aalrubayyi@wtco.com.sa',N'adaldossari@wtco.com.sa'),
	 (N'aalhawish@wtco.com.sa',N'malhommedi@wtco.com.sa'),
	 (N'aalhawish@wtco.com.sa',N'kalghanim@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalhawish@wtco.com.sa',N'halaseel@wtco.com.sa'),
	 (N'aalhawish@wtco.com.sa',N'halmushref@wtco.com.sa'),
	 (N'aabuhezam@wtco.com.sa',N'aahalghamdi@wtco.com.sa'),
	 (N'amalshamrani@wtco.com.sa',N'maalshalan@wtco.com.sa'),
	 (N'talrumayh@wtco.com.sa',N'aalrubayyi@wtco.com.sa'),
	 (N'talrumayh@wtco.com.sa',N'kalhuzami@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'faldossary@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'ahalharbi@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'falubaidi@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'aalmurdhi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'halshehry@wtco.com.sa',N'salowayyid@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'malsubey@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'ahalqarni@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'aaalshamrani@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'ibuubaid@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'haljradh@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'aalyamani@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'salilyan@wtco.com.sa'),
	 (N'halshehry@wtco.com.sa',N'jaldawsari@wtco.com.sa'),
	 (N'abdulrahmanalharbi@wtco.com.sa',N'halshehry@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'abdulrahmanalharbi@wtco.com.sa',N'gudin@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'nalwisaifer@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'mhaaloufi@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'aalmusailm@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'walkhathami@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'malzuriq@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'fmalghamdi@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'wabuallh@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'malradhwan@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'abdulrahmanalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aahassan@wtco.com.sa',N'aabuhezam@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'salabood@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'kalzaben@wtco.com.sa'),
	 (N'aahassan@wtco.com.sa',N'kalbakheet@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'aalroished@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'amaldossary@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'ralsunayni@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'awalsubaiei@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'yaldookhi@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'falgroon@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malradhwan@wtco.com.sa',N'mubarakaldossary@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'ssalkhaldi@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'salzahib@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'maldosaary@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'aalrasasi@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'nalmohishil@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'balkhaldi@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'jholgado@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'malomran@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'aalghwenm@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malradhwan@wtco.com.sa',N'kalkhaldi@wtco.com.sa'),
	 (N'malradhwan@wtco.com.sa',N'kmalghamdi@wtco.com.sa'),
	 (N'malturki@wtco.com.sa',N'malquwaydiri@wtco.com.sa'),
	 (N'malturki@wtco.com.sa',N'mkaldossary@wtco.com.sa'),
	 (N'malturki@wtco.com.sa',N'aalrewished@wtco.com.sa'),
	 (N'malturki@wtco.com.sa',N'aalbaloshi@wtco.com.sa'),
	 (N'malturki@wtco.com.sa',N'aalabdulqadir@wtco.com.sa'),
	 (N'malshaikh@wtco.com.sa',N'talrumayh@wtco.com.sa'),
	 (N'malshaikh@wtco.com.sa',N'szeya@wtco.com.sa'),
	 (N'malgwaizani@wtco.com.sa',N'mabaldossary@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malgwaizani@wtco.com.sa',N'oalkhalaf@wtco.com.sa'),
	 (N'malgwaizani@wtco.com.sa',N'nalsuwainea@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalowais@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'salhamdan@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalkhalifah@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'salkawi@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalwhaimed@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'ksalghamdi@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalrawa@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'baldousari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ealawami@wtco.com.sa',N'salqawba@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'yboqarsin@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'malsubeai@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'afalgi@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'mohammedaldossary@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'raldossary@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'malshmrei@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'hyalsaqer@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'faljuadan@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalrajab@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ealawami@wtco.com.sa',N'ralmsalam@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'aalmohsen@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'faalqahtani@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'malabdulateef@wtco.com.sa'),
	 (N'ealawami@wtco.com.sa',N'kalowis@wtco.com.sa'),
	 (N'jalmakki@wtco.com.sa',N'maldossri@wtco.com.sa'),
	 (N'aalzaid@wtco.com.sa',N'ialqarawi@wtco.com.sa'),
	 (N'aalzaid@wtco.com.sa',N'mabnmi@wtco.com.sa'),
	 (N'aalzaid@wtco.com.sa',N'oalbulayhid@wtco.com.sa'),
	 (N'aalzaid@wtco.com.sa',N'kalosaimi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalzaid@wtco.com.sa',N'balbadah@wtco.com.sa'),
	 (N'malmoharib@wtco.com.sa',N'nalomran@wtco.com.sa'),
	 (N'malmoharib@wtco.com.sa',N'aaljaelan@wtco.com.sa'),
	 (N'aaaldossary@wtco.com.sa',N'aalmuqteeb@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'mialharbi@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'snalmutairi@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'ralmutairi@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'mawalshammari@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'malqarzaee@wtco.com.sa'),
	 (N'galmutairi@wtco.com.sa',N'malreshidi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'akalmutairi@wtco.com.sa',N'mabalotaibi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'yalmutairi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'aaalmutairi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'malrashdi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'abdullahlaalharbi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'salzenidi@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'maltasan@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'aaltasan@wtco.com.sa'),
	 (N'akalmutairi@wtco.com.sa',N'aawidh@wtco.com.sa'),
	 (N'salrabea@wtco.com.sa',N'aealharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salrabea@wtco.com.sa',N'aaljasim@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'fshalmutairi@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'halnaim@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'yalmater@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'haalsuleea@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'nhalotaibi@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'amealmutairi@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'ralrayes@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'mansouraldawsari@wtco.com.sa'),
	 (N'mubarakaldawsari@wtco.com.sa',N'aealotaibi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'zalhassan@wtco.com.sa',N'aalnwiser@wtco.com.sa'),
	 (N'zalhassan@wtco.com.sa',N'aalzeed@wtco.com.sa'),
	 (N'zalhassan@wtco.com.sa',N'salkulaib@wtco.com.sa'),
	 (N'taldahash@wtco.com.sa',N'malquraysh@wtco.com.sa'),
	 (N'taldahash@wtco.com.sa',N'aaalsaeed@wtco.com.sa'),
	 (N'taldahash@wtco.com.sa',N'kalkulaib@wtco.com.sa'),
	 (N'taldahash@wtco.com.sa',N'eoalharbi@wtco.com.sa'),
	 (N'taldahash@wtco.com.sa',N'aalfaraj@wtco.com.sa'),
	 (N'abdullahalthawadi@wtco.com.sa',N'aealdossary@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'aalsalman@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mhoalharbi@wtco.com.sa',N'mabalharbi@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'halsaleam@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'malblaihi@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'malhawil@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'malbogme@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'halrasheedi@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'malklafi@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'aaldarwish@wtco.com.sa'),
	 (N'mhoalharbi@wtco.com.sa',N'malafiyah@wtco.com.sa'),
	 (N'falotibe@wtco.com.sa',N'msalotaibi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'falotibe@wtco.com.sa',N'maleanzi@wtco.com.sa'),
	 (N'falotibe@wtco.com.sa',N'aalbagshi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'aalalghamdi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'kmalharbi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'malmoharib@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'balsabi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'aaalosaimi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'oalghamdi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'ahmadalthawadi@wtco.com.sa'),
	 (N'saltkhifi@wtco.com.sa',N'ialbaqami@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'saltkhifi@wtco.com.sa',N'ftarmeen@wtco.com.sa'),
	 (N'aealharbi@wtco.com.sa',N'mohammedaldosari@wtco.com.sa'),
	 (N'salutaibi@wtco.com.sa',N'memsalim@wtco.com.sa'),
	 (N'salutaibi@wtco.com.sa',N'maljomah@wtco.com.sa'),
	 (N'salutaibi@wtco.com.sa',N'ssalanazi@wtco.com.sa'),
	 (N'salutaibi@wtco.com.sa',N'nalarajinah@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'kalmunaysir@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'salotaibi@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'mbamousa@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'aalsulami@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'nalarajinah@wtco.com.sa',N'sbalharth@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'abdulazizasiri@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'mawwad@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'malsaeed@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'aalabdullah@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'yalsulaiman@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'aalsubaiei@wtco.com.sa'),
	 (N'nalarajinah@wtco.com.sa',N'mazalshammari@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'ahalmarri@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'mhoalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmozine@wtco.com.sa',N'apaul@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'rabhilash@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'balghamdi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'falmlaqi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'saltkhifi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'malfardan@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'aalnaji@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'balsufayan@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'galmutairi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'aalameer@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmozine@wtco.com.sa',N'baldarwish@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'halhajji@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'ealawami@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'sbuqursayn@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'prao@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'saloslab@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'maleziab@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'zalabdulaziz@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'naalharbi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'tawalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmozine@wtco.com.sa',N'salutaibi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'naldawsary@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'mshwihiy@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'rhalqahtani@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'balasmari@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'salzaylaee@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'talharbi@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'aalasiri@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'taljohani@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'nsalzahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmozine@wtco.com.sa',N'salhamazani@wtco.com.sa'),
	 (N'aalmozine@wtco.com.sa',N'asasalbati@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'amoalmalki@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'bnalharbi@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'ndalotaibi@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'malharagen@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'maljossari@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'malmas@wtco.com.sa'),
	 (N'salquwayfili@wtco.com.sa',N'falaradi@wtco.com.sa'),
	 (N'naldawsary@wtco.com.sa',N'abosilom@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'naldawsary@wtco.com.sa',N'falhagri@wtco.com.sa'),
	 (N'naldawsary@wtco.com.sa',N'nalyami@wtco.com.sa'),
	 (N'naldawsary@wtco.com.sa',N'yalzaeebi@wtco.com.sa'),
	 (N'naldawsary@wtco.com.sa',N'salbarahim@wtco.com.sa'),
	 (N'mshwihiy@wtco.com.sa',N'falharbi@wtco.com.sa'),
	 (N'mshwihiy@wtco.com.sa',N'balshahrani@wtco.com.sa'),
	 (N'mshwihiy@wtco.com.sa',N'asahhari@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'iabdulsalam@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'msalanazi@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'falsaooy@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalshaikhi@wtco.com.sa',N'malmsnad@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'fahadnaalharbi@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'salabdulkarim@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'faisalfaalharbi@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'smalharbi@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'aaljabali@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'maljumah@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'malrawgi@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'salashqan@wtco.com.sa'),
	 (N'aalshaikhi@wtco.com.sa',N'falhaidari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'zalabdulaziz@wtco.com.sa',N'abursheed@wtco.com.sa'),
	 (N'zalabdulaziz@wtco.com.sa',N'halgallaf@wtco.com.sa'),
	 (N'zalabdulaziz@wtco.com.sa',N'aalasfoor@wtco.com.sa'),
	 (N'ssalanazi@wtco.com.sa',N'fallhaidan@wtco.com.sa'),
	 (N'ssalanazi@wtco.com.sa',N'oalsohaibani@wtco.com.sa'),
	 (N'ssalanazi@wtco.com.sa',N'salquwayfili@wtco.com.sa'),
	 (N'maljomah@wtco.com.sa',N'malmulayfi@wtco.com.sa'),
	 (N'maljomah@wtco.com.sa',N'oalmotari@wtco.com.sa'),
	 (N'maljomah@wtco.com.sa',N'ialmoshel@wtco.com.sa'),
	 (N'tawalharbi@wtco.com.sa',N'nalmuhsen@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'tawalharbi@wtco.com.sa',N'aalaradi@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'alialshehri@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'bhalajmi@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'ayaghmour@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'kalsayhati@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'amofareh@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'aalzaid@wtco.com.sa'),
	 (N'saloslab@wtco.com.sa',N'salrabea@wtco.com.sa'),
	 (N'jalsaleh@wtco.com.sa',N'aalmarfoei@wtco.com.sa'),
	 (N'jalsaleh@wtco.com.sa',N'malharajin@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'jalsaleh@wtco.com.sa',N'mhaloufi@wtco.com.sa'),
	 (N'jalsaleh@wtco.com.sa',N'maldosery@wtco.com.sa'),
	 (N'jalsaleh@wtco.com.sa',N'aaldosari@wtco.com.sa'),
	 (N'jalsaleh@wtco.com.sa',N'falhrajen@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'ralmarri@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'dalnabani@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'smaalmarri@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'talisa@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'ahalkhaldi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'mhalshehri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salashqan@wtco.com.sa',N'kalmohaimeed@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'jalsaleh@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'saldosariy@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malhumim@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aalmustaneer@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malhagri@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'faldosery@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aaaldossary@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aalsulaiman@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'akalmutairi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salashqan@wtco.com.sa',N'jalmakki@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aaldossari@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malfaraj@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'bmalajmi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'ahalhajri@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'anamazi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'mzalqahtani@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'saldossary@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'falotibe@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malmutyiri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salashqan@wtco.com.sa',N'ialresheedi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'maalhumaid@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'mubarakaldawsari@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'bmalhumaid@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'matalmutairi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malajmi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'amalajmi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'zalhassan@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aalbaqshi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'nalamri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salashqan@wtco.com.sa',N'taldahash@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'falzahrani@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'ssalghamdi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'malnaem@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aabalzahrani@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'kalghamdi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'asalahmari@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'amalghamdi@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'salmarbae@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'jalsmairi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salashqan@wtco.com.sa',N'malhabib@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'oalmarwani@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'kalmalki@wtco.com.sa'),
	 (N'salashqan@wtco.com.sa',N'aealanazi@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'kalsaleh@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'kalasiri@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'esharafi@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'ymasha@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'yalshahrani@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'saqis@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malamri@wtco.com.sa',N'amirqami@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'ahabbash@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'hsalqahtani@wtco.com.sa'),
	 (N'malamri@wtco.com.sa',N'salawad@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'aalyami@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'hqhtani@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'aalmousa@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'aalhoukash@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'saalqahtani@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'mrobia@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malsaree@wtco.com.sa',N'malbuesis@wtco.com.sa'),
	 (N'malsaree@wtco.com.sa',N'mohamadalqahtani@wtco.com.sa'),
	 (N'iageli@wtco.com.sa',N'mabassiri@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'aawad@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'ialkhayat@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'fgossadi@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'akamli@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'halhazmi@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'mhhakami@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'mjaafari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mhaidari@wtco.com.sa',N'yshafea@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'oalamer@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'adagrere@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'kmajari@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'kmoafa@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'kaqeel@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'fsawadi@wtco.com.sa'),
	 (N'mhaidari@wtco.com.sa',N'aalshiekhi@wtco.com.sa'),
	 (N'hbaraee@wtco.com.sa',N'yageeli@wtco.com.sa'),
	 (N'hbaraee@wtco.com.sa',N'ejafary@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'hbaraee@wtco.com.sa',N'ihallush@wtco.com.sa'),
	 (N'hbaraee@wtco.com.sa',N'yamshqeqi@wtco.com.sa'),
	 (N'hbaraee@wtco.com.sa',N'azainaldeen@wtco.com.sa'),
	 (N'hbaraee@wtco.com.sa',N'aaalhelali@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'jalhelali@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'ssaalqahtani@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'imasud@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'hjeraibi@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'kmasha@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'fhamed@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'agosadi@wtco.com.sa',N'msufyani@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'halkathiri@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'ealmojam@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'abazzari@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'iquzi@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'hdighriri@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'momar@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'mmudarbish@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'fhakami@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'anahari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'agosadi@wtco.com.sa',N'smahzari@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'yayed@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'sharbi@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'mallan@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'akohrmy@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'mmulayhi@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'ohakami@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'mkaabi@wtco.com.sa'),
	 (N'agosadi@wtco.com.sa',N'aelaqi@wtco.com.sa'),
	 (N'amojathel@wtco.com.sa',N'yalasiri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amojathel@wtco.com.sa',N'mhualfaifi@wtco.com.sa'),
	 (N'amojathel@wtco.com.sa',N'malsagoor@wtco.com.sa'),
	 (N'aalmodra@wtco.com.sa',N'farrajalqahtani@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'sabudash@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'agohal@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'halnami@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'rjaafari@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'rqaysi@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'maladani@wtco.com.sa'),
	 (N'aalfahimi@wtco.com.sa',N'amubarak@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mhamdi@wtco.com.sa',N'ootwady@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'iotudi@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'aalziyadi@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'hmubarak@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'hjouni@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'azalhadri@wtco.com.sa'),
	 (N'mhamdi@wtco.com.sa',N'mamubarak@wtco.com.sa'),
	 (N'aetwedi@wtco.com.sa',N'malbuhlul@wtco.com.sa'),
	 (N'aetwedi@wtco.com.sa',N'aaameri@wtco.com.sa'),
	 (N'aetwedi@wtco.com.sa',N'bbahri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aetwedi@wtco.com.sa',N'aialhelali@wtco.com.sa'),
	 (N'aetwedi@wtco.com.sa',N'aalkour@wtco.com.sa'),
	 (N'aetwedi@wtco.com.sa',N'yalhashmi@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'saseri@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'aqahtan@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'mzealie@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'malgamdi@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'ambakri@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'amalghamdi1@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'abdullahalasiri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalswaid@wtco.com.sa',N'abdulrahmanalshehri@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'abdulrahmanasiri@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'rmalqahtani@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'bassiri@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'aalasmari@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'jalsalem@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'eabumismar@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'maalqarni@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'amutmi@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'shalnasser@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalswaid@wtco.com.sa',N'amalhelali@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'amalqahtani@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'malsaree@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'halsharif@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'salmarie@wtco.com.sa'),
	 (N'aalswaid@wtco.com.sa',N'aliabasiri@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'maalshahrani@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'malghanmy@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'fmealqahtani@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'talalhareth@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'raladawi@wtco.com.sa',N'masiry@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'gasiri@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'smasiri@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'walshahrani@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'falshahrani@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'malgobesi@wtco.com.sa'),
	 (N'raladawi@wtco.com.sa',N'amalshahrani@wtco.com.sa'),
	 (N'ialfaifi@wtco.com.sa',N'aalmari@wtco.com.sa'),
	 (N'ialfaifi@wtco.com.sa',N'aalmodra@wtco.com.sa'),
	 (N'ialfaifi@wtco.com.sa',N'aalahmari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalsoma@wtco.com.sa',N'azalshahrani@wtco.com.sa'),
	 (N'aalsoma@wtco.com.sa',N'gmoaafa@wtco.com.sa'),
	 (N'mshowaihi@wtco.com.sa',N'hmakeen@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'whakami@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'mnahari@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'myasiri@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'aduhl@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'qqumayri@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'agamai@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'qalfaifi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'hbazzari@wtco.com.sa',N'falfifia@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'hhamag@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'kalsuhaymi@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'yabdullah@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'aahakami@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'ahamzi@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'fkryry@wtco.com.sa'),
	 (N'hbazzari@wtco.com.sa',N'sjammali@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'afaqih@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'satwdi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalassiri@wtco.com.sa',N'mamar@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'amaasiri@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'aalkhuzayim@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'haboallam@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'ahmedasiri@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'aalkhashlan@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'ajasiri@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'mhagawi@wtco.com.sa'),
	 (N'aalassiri@wtco.com.sa',N'aeasiri@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'falalhmad@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaljabir@wtco.com.sa',N'kalassiri@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'yalzulayq@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'jalqahtani@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'aqalfifi@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'malbawwah@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'faisalalqahtani@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'ssaman@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'ahaalfaifi@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'msahhari@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'balshehri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaljabir@wtco.com.sa',N'abdulmajeedasiri@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'amadey@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'falfaifi@wtco.com.sa'),
	 (N'kaljabir@wtco.com.sa',N'fsalqahtani@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'ahilali@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'mhaidari@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'alimoasiri@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'asalnashri@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'asafhi@wtco.com.sa'),
	 (N'mmoti@wtco.com.sa',N'mhaldawsari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mjabri@wtco.com.sa',N'ahualfaifi@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'salghamdi@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'ialsabie@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'ajabri@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'aghaleti@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'ealhazmy@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'kalmakhloti@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'shuzaymi@wtco.com.sa'),
	 (N'mjabri@wtco.com.sa',N'majlan@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'ayalhadri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mettwadi@wtco.com.sa',N'mali@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'aliasiri@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'mqadari@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'qshamakhi@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'amalamri@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'fzuqayl@wtco.com.sa'),
	 (N'mettwadi@wtco.com.sa',N'ojmali@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'aaasiri@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'fmaalqahtani@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'aalaamri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amalamri@wtco.com.sa',N'misfralqahtani@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'ymasiri@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'kasiri@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'aalsalemy@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'yalseef@wtco.com.sa'),
	 (N'amalamri@wtco.com.sa',N'msalshahrani@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'mmabutaleb@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'fnajmi@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'sjabri@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'agofshi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalqaysi@wtco.com.sa',N'ijammali@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'mghubayn@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'yjumumi@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'hazeb@wtco.com.sa'),
	 (N'aalqaysi@wtco.com.sa',N'htharwan@wtco.com.sa'),
	 (N'mmalammari@wtco.com.sa',N'mmalqahtani@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'ashafei@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'aburaeed@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'marishi@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'ydufus@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amobarki@wtco.com.sa',N'hhatani@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'mhamdi@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'aetwedi@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'hbaraee@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'aalfahimi@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'daljumah@wtco.com.sa'),
	 (N'amobarki@wtco.com.sa',N'amialmalki@wtco.com.sa'),
	 (N'srasiri@wtco.com.sa',N'raladawi@wtco.com.sa'),
	 (N'srasiri@wtco.com.sa',N'aalqaysi@wtco.com.sa'),
	 (N'srasiri@wtco.com.sa',N'malamri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalayyafi@wtco.com.sa',N'mhaalfaifi@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'iobahri@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'sjahfali@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'kalhlaly@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'tawaji@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'ayedsaasiri@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'ntayran@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'aalkhairi@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'maalasiri@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'mmomajrashi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalayyafi@wtco.com.sa',N'mjabri@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'ahbahri@wtco.com.sa'),
	 (N'aalayyafi@wtco.com.sa',N'asarhan@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'ayalshahrani@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'abdulazizalqahtani@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'mqahtani@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'aarishi@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'alimasiri@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'aabalshehri@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'habutaleb@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ayedsaasiri@wtco.com.sa',N'asuhayli@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'mfaifi@wtco.com.sa'),
	 (N'ayedsaasiri@wtco.com.sa',N'malasmari@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'motaif@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'mmalammari@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'aalsoma@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'mshowaihi@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'hbazzari@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'hzughlul@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'mahassiri@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalnami@wtco.com.sa',N'kaljabir@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'msaadi@wtco.com.sa'),
	 (N'aalnami@wtco.com.sa',N'aagile@wtco.com.sa'),
	 (N'thayjan@wtco.com.sa',N'mabalgarni@wtco.com.sa'),
	 (N'thayjan@wtco.com.sa',N'ahmadalshehri@wtco.com.sa'),
	 (N'thayjan@wtco.com.sa',N'msallah@wtco.com.sa'),
	 (N'thayjan@wtco.com.sa',N'falmalki@wtco.com.sa'),
	 (N'thayjan@wtco.com.sa',N'hfadhel@wtco.com.sa'),
	 (N'hmadkhali@wtco.com.sa',N'aalsalamei@wtco.com.sa'),
	 (N'hmadkhali@wtco.com.sa',N'mdarban@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'hmadkhali@wtco.com.sa',N'aalassiri@wtco.com.sa'),
	 (N'aalrajhi@wtco.com.sa',N'agosadi@wtco.com.sa'),
	 (N'aalrajhi@wtco.com.sa',N'amojathel@wtco.com.sa'),
	 (N'aalrajhi@wtco.com.sa',N'ykhuzaee@wtco.com.sa'),
	 (N'aalrajhi@wtco.com.sa',N'thayjan@wtco.com.sa'),
	 (N'aalrajhi@wtco.com.sa',N'faltalhayyh@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'abdulmuhsenalqahtani@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'malahmari@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'asalqahtani@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'aalqarrash@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'faltalhayyh@wtco.com.sa',N'yabasiri@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'ayedmoasiri@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'habalmalki@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'mabasiri@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'balhashar@wtco.com.sa'),
	 (N'faltalhayyh@wtco.com.sa',N'tasiri@wtco.com.sa'),
	 (N'msaadi@wtco.com.sa',N'aibahri@wtco.com.sa'),
	 (N'aagile@wtco.com.sa',N'salhalawi@wtco.com.sa'),
	 (N'zmuawwadh@wtco.com.sa',N'zabalharbi@wtco.com.sa'),
	 (N'ahamad@wtco.com.sa',N'aalmajnuni@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ahamad@wtco.com.sa',N'malosaimi@wtco.com.sa'),
	 (N'ahamad@wtco.com.sa',N'sgalzahrani@wtco.com.sa'),
	 (N'ahamad@wtco.com.sa',N'hhalzahrani@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'htayb@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'tghomari@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'abaghalf@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'mmaghfuri@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'aaljaid@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'amahmoudi@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'aoalshamrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'saljuhani@wtco.com.sa',N'bbafaraj@wtco.com.sa'),
	 (N'saljuhani@wtco.com.sa',N'akeyari@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'walyahyawi@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'falmutairi@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'midris@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'aalaklabi@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'malzhrani@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'abarayan@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'bohawsawi@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'aalmasoudi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalasmi@wtco.com.sa',N'ralkhoshi@wtco.com.sa'),
	 (N'aalasmi@wtco.com.sa',N'haalghamdi@wtco.com.sa'),
	 (N'HAALHARBI@wtco.com.sa',N'mofallata@wtco.com.sa'),
	 (N'HAALHARBI@wtco.com.sa',N'ameqibl@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'amaljadani@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'hdashi@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'ajalshehri@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'mbadagish@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'afarhan@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'ralmaimoni@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'analbaqami@wtco.com.sa',N'alamash@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'falgrani@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'aaalmalki@wtco.com.sa'),
	 (N'analbaqami@wtco.com.sa',N'malzahraniu@wtco.com.sa'),
	 (N'ameqibl@wtco.com.sa',N'salhuzali@wtco.com.sa'),
	 (N'ameqibl@wtco.com.sa',N'sbakayli@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'shadydi@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'aealmalki@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'aalmasrei@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'falsaadi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malazwari@wtco.com.sa',N'ahmadmoalghamdi@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'ahalamri@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'oalgamdi@wtco.com.sa'),
	 (N'malazwari@wtco.com.sa',N'aalobaidi@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'yalharthi@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'snalhasani@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'asalzhrani@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'malshareef@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'aaaltalhi@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'aalbarqi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'afalmalki@wtco.com.sa',N'aqalsubhi@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'ksalzahrani@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'falhajery@wtco.com.sa'),
	 (N'afalmalki@wtco.com.sa',N'faalotaibi@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'malzahrani@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'nalethainani@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'abdulrahimalghamdi@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'fhalotaibi@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'abdullahsaalghamdi@wtco.com.sa'),
	 (N'aalharthi@wtco.com.sa',N'bjawahraji@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'gbadr@wtco.com.sa',N'malsaegh@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'falsuwayhiri@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'kfallatah@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'ralsulaimani@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'mnalhazmi@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'ahawsawi@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'mbfallata@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'amhakami@wtco.com.sa'),
	 (N'gbadr@wtco.com.sa',N'malmuwalad@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'salsindi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalmowallad@wtco.com.sa',N'ealmahmmodi@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'ealghamdi@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'talemery@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'ralmuolad@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'tsawadi@wtco.com.sa'),
	 (N'kalmowallad@wtco.com.sa',N'balsayed@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'mallqmany@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'muathalghamdi@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'salsulami@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'ahaljadani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mhejazi@wtco.com.sa',N'salmakhshami@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'amashraee@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'falnaseri@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'omouathen@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'mmamajrashi@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'amfallatah@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'analshamrani@wtco.com.sa'),
	 (N'mhejazi@wtco.com.sa',N'aalazwari@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'aalsulamy@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'hhalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmalki@wtco.com.sa',N'asalzahrani@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'amanqari@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'fbhari@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'fjaafari@wtco.com.sa'),
	 (N'aalmalki@wtco.com.sa',N'ahanthal@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'yallahianiu@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'smalghamdi@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'mallohaibe@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'halotaibi@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'halqurashi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'abadi@wtco.com.sa',N'aahalharbi@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'mnaji@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'talsufyani@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'aalthubaiti@wtco.com.sa'),
	 (N'abadi@wtco.com.sa',N'faltowirgi@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'maalammari@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'maladwani@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'aalammari@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'sbaazeem@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'osualharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'asalman@wtco.com.sa',N'mfallatah@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'mmushayni@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'mallahyani@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'yalhosawy@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'iahamdi@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'salzhrani@wtco.com.sa'),
	 (N'asalman@wtco.com.sa',N'smaqhrabi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'mbahareth@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'mhabutaleb@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'walsufyani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'hahalmalki@wtco.com.sa',N'asimalharthi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'asamadani@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'nalharthi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'sfalhasani@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'kalshehri@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'aaltalhi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'ralahdali@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'maltowairgi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'ralharthi@wtco.com.sa'),
	 (N'hahalmalki@wtco.com.sa',N'kalhejji@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'halbusaysi@wtco.com.sa',N'falwaal@wtco.com.sa'),
	 (N'halbusaysi@wtco.com.sa',N'yalamri@wtco.com.sa'),
	 (N'halbusaysi@wtco.com.sa',N'malyamani@wtco.com.sa'),
	 (N'halbusaysi@wtco.com.sa',N'akhuzaee@wtco.com.sa'),
	 (N'halbusaysi@wtco.com.sa',N'yotif@wtco.com.sa'),
	 (N'nalmalki@wtco.com.sa',N'malazwari@wtco.com.sa'),
	 (N'nalmalki@wtco.com.sa',N'abadi@wtco.com.sa'),
	 (N'faljuaid@wtco.com.sa',N'salhusayni@wtco.com.sa'),
	 (N'faljuaid@wtco.com.sa',N'aalharthi@wtco.com.sa'),
	 (N'faljuaid@wtco.com.sa',N'saljuhani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'akilani@wtco.com.sa',N'ahamad@wtco.com.sa'),
	 (N'akilani@wtco.com.sa',N'zmuawwadh@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'aalhothali@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'ifallatah@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'smalke@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'falhseni@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'amoalhazmi@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'aaljdany@wtco.com.sa'),
	 (N'hfelemban@wtco.com.sa',N'aalmaimany@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'babuhumrah@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'fbadughaish@wtco.com.sa',N'myalqarni@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'mtayyeb@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'faljuaid@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'faisalnaalharbi@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'hfelemban@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'aalamoudi@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'sbadhawi@wtco.com.sa'),
	 (N'fbadughaish@wtco.com.sa',N'aalbukhari@wtco.com.sa'),
	 (N'walmashyakhi@wtco.com.sa',N'salqurashi@wtco.com.sa'),
	 (N'walmashyakhi@wtco.com.sa',N'iawaji@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'sshalzahrani@wtco.com.sa',N'falsofiane@wtco.com.sa'),
	 (N'sshalzahrani@wtco.com.sa',N'baltalhi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'ijawahi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'aalsabri@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'hmalghamdi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'nalmalki@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'ashoaib@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'aalmalke@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'aalmalki@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'afalmalki@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amalnashri@wtco.com.sa',N'mhejazi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'analbaqami@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'aalasmi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'ahalamoudi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'fsageer@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'malhazmi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'abarnawi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'malnasheri@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'asalghamdi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'falrashidi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amalnashri@wtco.com.sa',N'faalharbi@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'ralnakhli@wtco.com.sa'),
	 (N'amalnashri@wtco.com.sa',N'asaalzahrani@wtco.com.sa'),
	 (N'fallohybi@wtco.com.sa',N'ryalharbi@wtco.com.sa'),
	 (N'fallohybi@wtco.com.sa',N'maledeil@wtco.com.sa'),
	 (N'fallohybi@wtco.com.sa',N'araqwani@wtco.com.sa'),
	 (N'fallohybi@wtco.com.sa',N'ialmaqadi@wtco.com.sa'),
	 (N'fallohybi@wtco.com.sa',N'halfrasani@wtco.com.sa'),
	 (N'halahmari@wtco.com.sa',N'faljuhani@wtco.com.sa'),
	 (N'halahmari@wtco.com.sa',N'abadegaish@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'halahmari@wtco.com.sa',N'tamalharbi@wtco.com.sa'),
	 (N'halahmari@wtco.com.sa',N'kalsfri@wtco.com.sa'),
	 (N'halahmari@wtco.com.sa',N'imahmoud@wtco.com.sa'),
	 (N'halahmari@wtco.com.sa',N'foalharbi@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'gbadr@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'aralbaqami@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'saldhuwayhi@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'ssabagh@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'esughayyir@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'balzahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salhusayni@wtco.com.sa',N'aalqhuraibi@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'hsalsharif@wtco.com.sa'),
	 (N'salhusayni@wtco.com.sa',N'amalosaimi@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'falsaid@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'salhssani@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'falghanmi@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'talsalmi@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'ralandijany@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'bmoalharbi@wtco.com.sa'),
	 (N'hmalghamdi@wtco.com.sa',N'malaryani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'myalqarni@wtco.com.sa',N'kalmowallad@wtco.com.sa'),
	 (N'myalqarni@wtco.com.sa',N'asalman@wtco.com.sa'),
	 (N'myalqarni@wtco.com.sa',N'halbusaysi@wtco.com.sa'),
	 (N'myalqarni@wtco.com.sa',N'sshalzahrani@wtco.com.sa'),
	 (N'babuhumrah@wtco.com.sa',N'smalzahrani@wtco.com.sa'),
	 (N'babuhumrah@wtco.com.sa',N'mkalqahtani@wtco.com.sa'),
	 (N'smalzahrani@wtco.com.sa',N'walmashyakhi@wtco.com.sa'),
	 (N'smalzahrani@wtco.com.sa',N'halahmari@wtco.com.sa'),
	 (N'smalzahrani@wtco.com.sa',N'hahalmalki@wtco.com.sa'),
	 (N'smalzahrani@wtco.com.sa',N'kaalzahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ijawahi@wtco.com.sa',N'halmuntashiri@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'salfahmi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'kalhuzali@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'amalhazmi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'aalthubyani@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'ahmadsaalghamdi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'tmalharbi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'kmoshaini@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'asharahili@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'khalghamdi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ijawahi@wtco.com.sa',N'abdulrahmanalharthi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'nalsaedi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'aharthi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'rhalharbi@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'hbukhari@wtco.com.sa'),
	 (N'ijawahi@wtco.com.sa',N'knalghamdi@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'hsughayyir@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'kalsufyani@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'aalkorashy@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'aalmutmi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalsabri@wtco.com.sa',N'ajestania@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'aalsulaimani@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'saalzahrani@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'akhawjah@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'salsharif@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'falharthi@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'aafallatah@wtco.com.sa'),
	 (N'aalsabri@wtco.com.sa',N'saalmalki@wtco.com.sa'),
	 (N'iialharbi@wtco.com.sa',N'falhakami@wtco.com.sa'),
	 (N'iialharbi@wtco.com.sa',N'mhmalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salahmadi@wtco.com.sa',N'malarawi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'rmazi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'mmualharbi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'mgharsah@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'esarhan@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'oalqarni@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'malalawi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'msubah@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'aaalzhrani@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'smousa@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salahmadi@wtco.com.sa',N'hbanassar@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'sbakhraiba@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'raljadani@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'amiski@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'malbishri@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'oalbarakati@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'malali@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'fbaroom@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'balofie@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'ybalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'salahmadi@wtco.com.sa',N'salsubhei@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'ealowide@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'maljezawi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'yalghamdi@wtco.com.sa'),
	 (N'salahmadi@wtco.com.sa',N'salenezi@wtco.com.sa'),
	 (N'talmohammadi@wtco.com.sa',N'salahmadi@wtco.com.sa'),
	 (N'talmohammadi@wtco.com.sa',N'aalkattan@wtco.com.sa'),
	 (N'talmohammadi@wtco.com.sa',N'mbarayan@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'bbagour@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'yalshikhy@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mbarayan@wtco.com.sa',N'smalmutairi@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'walsuryahy@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'aaalqarni@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'walsubhi@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'sbaeisa@wtco.com.sa'),
	 (N'mbarayan@wtco.com.sa',N'ralhazmi@wtco.com.sa'),
	 (N'aalkattan@wtco.com.sa',N'aabalyoubi@wtco.com.sa'),
	 (N'aalkattan@wtco.com.sa',N'balyoubi@wtco.com.sa'),
	 (N'aalkattan@wtco.com.sa',N'aalindunosi@wtco.com.sa'),
	 (N'aalkattan@wtco.com.sa',N'halzanbagi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalkattan@wtco.com.sa',N'malmidhwah@wtco.com.sa'),
	 (N'aalkattan@wtco.com.sa',N'malkhamisei@wtco.com.sa'),
	 (N'malayed@wtco.com.sa',N'nalhanaya@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'mbasiony@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'hbadawy@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'falhargan@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'hmohamed@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'malduraywish@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'mgowied@wtco.com.sa'),
	 (N'sbalateef@wtco.com.sa',N'rhalzahrani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalghanim@wtco.com.sa',N'aalthinayan@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'talanazi@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'halatawi@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'habuzaid@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'whachicha@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'bahmed@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'kalmuqbil@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'oalmutairi@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'balanazi@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'nalessa@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalghanim@wtco.com.sa',N'malarfaj@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'aalmosa@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'ralsamrin@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'yalsamman@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'smoinuddinm@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'halsahli@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'lalhubail@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'abadsha@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'malothman@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'nalsaloumi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalghanim@wtco.com.sa',N'dalablani@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'malqahtani@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'aalshaer@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'nahalharbi@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'halbednah@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'saalshammari@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'amahmoud@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'hghaffar@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'nalhathal@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'aalmutaywia@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalghanim@wtco.com.sa',N'hchan@wtco.com.sa'),
	 (N'aalghanim@wtco.com.sa',N'mmibrahim@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'mafzal@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'agad@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'adanicic@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'mvya@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'ssekharan@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'mfalmutairi@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'msiddiqui@wtco.com.sa'),
	 (N'kaljandal@wtco.com.sa',N'aalrusayyis@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaljandal@wtco.com.sa',N'malayed@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'aaalshehri@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'omalghamdi@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'asalshareef@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'ayedhalzahrani@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'malharbi@wtco.com.sa'),
	 (N'salbeshri@wtco.com.sa',N'ealsulami@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'mjalal@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'malshekhmubarak@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'malmohaya@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'talfaris@wtco.com.sa',N'amalik@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'kalkhalid@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'salrabiah@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'abensbiet@wtco.com.sa'),
	 (N'talfaris@wtco.com.sa',N'malmilhim@wtco.com.sa'),
	 (N'anegm@wtco.com.sa',N'dbinslaeem@wtco.com.sa'),
	 (N'anegm@wtco.com.sa',N'hjomaa@wtco.com.sa'),
	 (N'anegm@wtco.com.sa',N'aeid@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'yalyahya@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'rturjoman@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalshibl@wtco.com.sa',N'salkanhal@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'aalqarni@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'masiri@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'aalmohaimeed@wtco.com.sa'),
	 (N'kaalshibl@wtco.com.sa',N'malsayari@wtco.com.sa'),
	 (N'balraouji@wtco.com.sa',N'malmasad@wtco.com.sa'),
	 (N'balraouji@wtco.com.sa',N'saalotaibi@wtco.com.sa'),
	 (N'balraouji@wtco.com.sa',N'malthaqeb@wtco.com.sa'),
	 (N'balraouji@wtco.com.sa',N'kaalmutairi@wtco.com.sa'),
	 (N'balraouji@wtco.com.sa',N'ealhamandi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'balraouji@wtco.com.sa',N'ybinsalmah@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'yalsohaim@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Kaldoulah@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Aalkhamis@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Mbadahdah@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Mbasalama@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Otarmoom@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Anassif@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Ahalshehri@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Aalhaqbani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'Salsaud@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'Salshabanat@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'oalokali@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'zhamdan@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'aalazzaz@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'sahmed@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'SALATHMI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'HALAHMADI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALRASHED@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'WGADERI@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'YALYOUSIF@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'OALSUBHI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AAZAYBI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'ABUQARSAIN@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'NHIFZI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'HALTHUQBI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AAALABDULLAH@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'WALDOSSARY@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'IALSHAQHA@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'KALHAMDAN@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'EALJAEDAN@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALBAWARDI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'HAALHADI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'BKALBUBADER@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'RALBAKHEEIT@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALTHUBYANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALMADEH@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'IALMEJALY@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'SAALSAYED@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'YALAMEER@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'OALNAMRY@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'RALWSIBI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALAKROUSH@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'SALGAREEB@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALRAMADAN@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALSUBAIE@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AGHALAB@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALSUBHI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'SALANAZI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'HALJOHANI@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'YAlJAZIA@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALAKLUBI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'SALMALKI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALHUWAYJI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALSHUHAYB@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALJEHANEI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALBUHLUL@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALHAMMADI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'RALQAHTANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALSHAMMERE@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'AALSULAITEEN@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AASSIRY@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'BALSOBHI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'JALMUBARAK@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALHOMIDAN@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALOKLBI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALHODAR@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'KALOBAID@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'ASAALQAHTANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'KALMUZIL@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'NALSHAHAB@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'NALDARWISH@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'MALASHUR@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AAALGHAMDI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALRASHEED@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FABDULGADOOS@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALSAFRI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AMOUSALAM@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FDALAHMADI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AMOALGHAMDI@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalwabel@wtco.com.sa',N'NALAMIR@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AALTHALABI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'FALJEDANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'NALJIHANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'AMOALZAHRANI@wtco.com.sa'),
	 (N'aalwabel@wtco.com.sa',N'aalshareef@wtco.com.sa'),
	 (N'yalqurashi@wtco.com.sa',N'aalshaikhi@wtco.com.sa'),
	 (N'yalqurashi@wtco.com.sa',N'aalmozine@wtco.com.sa'),
	 (N'yalqurashi@wtco.com.sa',N'abdullahalthawadi@wtco.com.sa'),
	 (N'yalqurashi@wtco.com.sa',N'balbubader@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'yalqurashi@wtco.com.sa',N'ialsenani@wtco.com.sa'),
	 (N'malshekhmubarak@wtco.com.sa',N'sarshad@wtco.com.sa'),
	 (N'aaalhussain@wtco.com.sa',N'drrengarajan@wtco.com.sa'),
	 (N'aaalhussain@wtco.com.sa',N'tabanumai@wtco.com.sa'),
	 (N'aaalhussain@wtco.com.sa',N'talabdulqader@wtco.com.sa'),
	 (N'ahaijan@wtco.com.sa',N'aalhadri@wtco.com.sa'),
	 (N'ahaijan@wtco.com.sa',N'amarir@wtco.com.sa'),
	 (N'ahaijan@wtco.com.sa',N'faalshahrani@wtco.com.sa'),
	 (N'ahaijan@wtco.com.sa',N'asharqi@wtco.com.sa'),
	 (N'ialawadh@wtco.com.sa',N'ahashad@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'mafzal@wtco.com.sa',N'halamer@wtco.com.sa'),
	 (N'agad@wtco.com.sa',N'aalshukr@wtco.com.sa'),
	 (N'agad@wtco.com.sa',N'aalmarshud@wtco.com.sa'),
	 (N'agad@wtco.com.sa',N'bmahmed@wtco.com.sa'),
	 (N'ysalghamdi@wtco.com.sa',N'kalhawas@wtco.com.sa'),
	 (N'ysalghamdi@wtco.com.sa',N'fmelibari@wtco.com.sa'),
	 (N'ysalghamdi@wtco.com.sa',N'rzamzami@wtco.com.sa'),
	 (N'ssekharan@wtco.com.sa',N'balharbi@wtco.com.sa'),
	 (N'ssekharan@wtco.com.sa',N'faloqayli@wtco.com.sa'),
	 (N'ssekharan@wtco.com.sa',N'zalharbi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'ssekharan@wtco.com.sa',N'Nkhaja@wtco.com.sa'),
	 (N'aalnafisah@wtco.com.sa',N'halmutairi@wtco.com.sa'),
	 (N'aalnafisah@wtco.com.sa',N'hjambi@wtco.com.sa'),
	 (N'aalnafisah@wtco.com.sa',N'osalqarni@wtco.com.sa'),
	 (N'aalnafisah@wtco.com.sa',N'aoalshehri@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'mmejdal@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'aalhudaif@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'aalobaid@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'saljohani@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'salbogami@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'aalmubrad@wtco.com.sa',N'nalshahrani@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'aalshahrani@wtco.com.sa'),
	 (N'aalmubrad@wtco.com.sa',N'ralnujaidi@wtco.com.sa'),
	 (N'malmodarra@wtco.com.sa',N'amohalghamdi@wtco.com.sa'),
	 (N'malmodarra@wtco.com.sa',N'yalmughamer@wtco.com.sa'),
	 (N'malsaiegh@wtco.com.sa',N'aoalbalawi@wtco.com.sa'),
	 (N'malsaiegh@wtco.com.sa',N'rbarry@wtco.com.sa'),
	 (N'malsaiegh@wtco.com.sa',N'fmalahmadi@wtco.com.sa'),
	 (N'malsaiegh@wtco.com.sa',N'malshantaf@wtco.com.sa'),
	 (N'malsaiegh@wtco.com.sa',N'akalkhalifah@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'malsaiegh@wtco.com.sa',N'aaalzahrani@wtco.com.sa'),
	 (N'mvya@wtco.com.sa',N'aaltamimi@wtco.com.sa'),
	 (N'adawarihas@wtco.com.sa',N'fsalharbi@wtco.com.sa'),
	 (N'adawarihas@wtco.com.sa',N'raalanazi@wtco.com.sa'),
	 (N'tkachouri@wtco.com.sa',N'fsaalharbi@wtco.com.sa'),
	 (N'tkachouri@wtco.com.sa',N'balraouji@wtco.com.sa'),
	 (N'tkachouri@wtco.com.sa',N'mjawad@wtco.com.sa'),
	 (N'tkachouri@wtco.com.sa',N'raldakhil@wtco.com.sa'),
	 (N'talkabsh@wtco.com.sa',N'talshaikh@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'halqahtani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'jalzahrani@wtco.com.sa',N'aalbaker@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'asalnakhli@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'talkabsh@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'malsaiegh@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'mshalsharif@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'oalshalan@wtco.com.sa'),
	 (N'jalzahrani@wtco.com.sa',N'walraddadi@wtco.com.sa'),
	 (N'kalkhalid@wtco.com.sa',N'salmutlaq@wtco.com.sa'),
	 (N'kalkhalid@wtco.com.sa',N'ralshehri@wtco.com.sa'),
	 (N'kalkhalid@wtco.com.sa',N'ialawadh@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kalkhalid@wtco.com.sa',N'malhisan@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'malsaad@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'aasiri@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'shalzahrani@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'aalshammari@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'falmulla@wtco.com.sa'),
	 (N'asalshammari@wtco.com.sa',N'maldawsari@wtco.com.sa'),
	 (N'oalnajm@wtco.com.sa',N'hahmedjey@wtco.com.sa'),
	 (N'oalnajm@wtco.com.sa',N'maalharbi@wtco.com.sa'),
	 (N'oalnajm@wtco.com.sa',N'ebakhet@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'oalnajm@wtco.com.sa',N'mmohiuddin@wtco.com.sa'),
	 (N'oalnajm@wtco.com.sa',N'melshazly@wtco.com.sa'),
	 (N'oalnajm@wtco.com.sa',N'aaljohany@wtco.com.sa'),
	 (N'aayaldawsari@wtco.com.sa',N'aalmohana@wtco.com.sa'),
	 (N'aayaldawsari@wtco.com.sa',N'taljoufi@wtco.com.sa'),
	 (N'aayaldawsari@wtco.com.sa',N'malmodarra@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'samulla@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'salribi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'adalghamdi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'jalmulhim@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalanazi@wtco.com.sa',N'ahalzahrani@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'ealsubaie@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'malrasheedi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'saalamri@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'fkaabi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalhathwal@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'halamri@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalthagafi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalshaiban@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'saalghamdi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalanazi@wtco.com.sa',N'nhowsawi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'ralzhrani@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalabdulqader@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalawadh@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'farishi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'ahaalshammari@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'zalkhaldi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'nmaldossary@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'talsubaie@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aaalqahtani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalanazi@wtco.com.sa',N'aalmulhem@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'ayalarawi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'habosaif@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'nalmirza@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'amalshamrani@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'baloshbah@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'balbagami@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'ialqooba@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'malkarri@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'shalqahtani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalanazi@wtco.com.sa',N'salfihani@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalahmed@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'salshaikhi@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'asaldawsari@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'yaldossary@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'hnalqahtani@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aalhawish@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'aabalharth@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'jalarbash@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'malturki@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'kaalanazi@wtco.com.sa',N'salsharari@wtco.com.sa'),
	 (N'kaalanazi@wtco.com.sa',N'talsultan@wtco.com.sa'),
	 (N'salrabiah@wtco.com.sa',N'aalduwaile@wtco.com.sa'),
	 (N'nalwisaifer@wtco.com.sa',N'sanalqahtani@wtco.com.sa'),
	 (N'nalwisaifer@wtco.com.sa',N'nalhebbi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'falhazmi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'kalahmari@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'rmoalqahtani@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'aaalameer@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'asalshammari@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'iabdulsalam@wtco.com.sa',N'falabdali@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'mmalghamdi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'aabalmutairi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'maljaloud@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'ialzedi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'tkushar@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'abinlibda@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'oaldalbahi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'falanazi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'yalotaibi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'iabdulsalam@wtco.com.sa',N'mfalqi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'aalabbas@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'aalsadiri@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'kalsulaitin@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'malwasmi@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'mobaarkaldossary@wtco.com.sa'),
	 (N'iabdulsalam@wtco.com.sa',N'aalameri@wtco.com.sa'),
	 (N'taljohani@wtco.com.sa',N'malalyani@wtco.com.sa'),
	 (N'abensbiet@wtco.com.sa',N'ralyahya@wtco.com.sa'),
	 (N'abensbiet@wtco.com.sa',N'ralshaibani@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'abensbiet@wtco.com.sa',N'malomari@wtco.com.sa'),
	 (N'fmalshammari@wtco.com.sa',N'nalabdaljabar@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'aateeg@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'halghelan@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'dshetty@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'halnafisee@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'talyahya@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'obinhameed@wtco.com.sa'),
	 (N'aalquraish@wtco.com.sa',N'salnowaiser@wtco.com.sa'),
	 (N'aalmohana@wtco.com.sa',N'salmohimmed@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'halqahtani@wtco.com.sa',N'kaleisa@wtco.com.sa'),
	 (N'halqahtani@wtco.com.sa',N'mabalqahtani@wtco.com.sa'),
	 (N'halqahtani@wtco.com.sa',N'mdkhan@wtco.com.sa'),
	 (N'bfalanazi@wtco.com.sa',N'aateeq@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalessa@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'skalharbi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'ealmutiri@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'talmohaimeed@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'kabuhomod@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalsahim@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'msalanazi@wtco.com.sa',N'ashalawi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalmalabi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'ialmuzael@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'yalshammri@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'ksalharbi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'azalanazi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'ayalrasheed@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'fhalharbi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalsubaei@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'msalhumaid@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'msalanazi@wtco.com.sa',N'nalmutairi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalabdulwahab@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'oaltuwijri@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'salbaiji@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'kkalanazi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'abukhari@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'malmutairi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aalalqarni@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'abdullahalqahtani@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'raltamimi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'msalanazi@wtco.com.sa',N'falmarri@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'balmutairi@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aoalshammari@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'fmalshammari@wtco.com.sa'),
	 (N'msalanazi@wtco.com.sa',N'aiaaljohani@wtco.com.sa'),
	 (N'taljoufi@wtco.com.sa',N'gkalmutairi@wtco.com.sa'),
	 (N'talmuzaini@wtco.com.sa',N'maldail@wtco.com.sa'),
	 (N'amalsultan@wtco.com.sa',N'aashour@wtco.com.sa'),
	 (N'amalsultan@wtco.com.sa',N'ralarfaj@wtco.com.sa'),
	 (N'amalsultan@wtco.com.sa',N'naaalhumaidi@wtco.com.sa');

	INSERT INTO #TheTable VALUES
	 (N'amalsultan@wtco.com.sa',N'smoalzahrani@wtco.com.sa'),
	 (N'lali@wtco.com.sa',N'aalmursy@wtco.com.sa'),
	 (N'lali@wtco.com.sa',N'nmahmud@wtco.com.sa');
