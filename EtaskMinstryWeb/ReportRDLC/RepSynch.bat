@ECHO OFF
PUSHD .
FOR /R %%d IN (.) DO (
    cd "%%d"
    IF EXIST *.rdl (
       copy *.rdl ..\\EtaskMinstry\ReportsRDLC\*.rdlc
    )
)
POPD
PUSHD .
FOR /R %%d IN (.) DO (
    cd "%%d"
    IF EXIST *.rdl (
       copy *.rdl ..\..\\EtaskMinstry\ReportsRDLC\*.rdlc
    )
)
POPD