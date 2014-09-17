nuget pack

rmdir build /s /q

nuget push *.nupkg -s http://61.191.190.177:9999/ A21D29EB-7E67-4581-96CA-5B6FB032D576

del *.nupkg

exit 0