set WORKSPACE=../..
set GEN_CLIENT=../Luban/Luban.dll

  dotnet %GEN_CLIENT% ^
      -t client ^
      -c cs-bin ^
      -d bin ^
      --conf ./luban.conf ^
      -x outputDataDir=%WORKSPACE%\Assets\Unity\Resources\Luban ^
      -x outputCodeDir=%WORKSPACE%\Assets\Demo\Luban\DataTable ^
      -x tableImporter.valueTypeNameFormat=Table{0}

  pause                              
