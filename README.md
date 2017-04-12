# clovershell client #
Client for hakchi mod (https://github.com/ClusterM/clovershell-daemon) which allows to access NES Mini's shell, execute commands and transfer files directly via USB, without UART and FEL.

## Usage ##

    clovershell shell [port]
    clovershell ftp [port]
    clovershell all [shell_port [ftp_port]] (default)    clovershell exec <command> [stdin [stdout [stderr]]]
    clovershell pull <remote_file> [local_file]
    clovershell push <local_file> <remote_file>

## Examples ##

    Start shell server on port 23:
     clovershell shell 23
    Start FTP server on port 21:
     clovershell ftp 21
    List files:
     clovershell exec "ls /etc/"
    Download file:
     clovershell pull /etc/inittab inittab
    Upload file:
     clovershell push inittab /etc/inittab
    Archive and download files:
     clovershell exec "cd /etc && tar -czv *" > file.tar.gz
    Archive and download files (alternative):
     clovershell exec "cd /etc && tar -czv *" null file.tar.gz
    Upload and extract files:
     clovershell exec "cd /etc && tar -xzv" file.tar.gz
    Upload and extract files (alternative):
     clovershell exec "cd /etc && tar -xzv" - <file.tar.gz
    Dump the whole decrypted filesystem:
     clovershell exec "dd if=/dev/mapper/root-crypt | gzip" > dump.img.gz
    Dump the whole decrypted filesystem (alternative):
     clovershell exec "dd if=/dev/mapper/root-crypt | gzip" null dump.img.gz
   
