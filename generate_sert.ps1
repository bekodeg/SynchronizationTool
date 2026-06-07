$cert = New-SelfSignedCertificate `
    -Subject "CN=synchronizationtool-sync" `
    -DnsName "synchronizationtool-sync" `
    -KeyAlgorithm RSA `
    -KeyLength 4096 `
    -NotAfter (Get-Date).AddYears(10) `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -Type SSLServerAuthentication

$password = ConvertTo-SecureString -String "YourPassword123" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath ".\certs\server.pfx" -Password $password
Export-Certificate -Cert $cert -FilePath ".\certs\server.crt" -Type CERT