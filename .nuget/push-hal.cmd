set p=..\AonWeb.FluentHttp.Hal\bin\Release\
for /f "tokens=*" %%a in ('dir %p%*.nupkg /b /od') do set newest=%%a
echo 
NuGet.exe push %p%%newest%  -source http://svd0nugetd01
