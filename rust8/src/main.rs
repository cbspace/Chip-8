use std::fs::File;
use std::io;

fn main() {
    let mut pc: u16 = 0;
    let mut vreg: Vec<u8> = vec![0; 16];
    let mut ireg: u16 = 0;

    let mut memory: Vec<u8> = vec![0; 0x10000];
    let mut stack: Vec<u16> = vec![];

    let rom_code = load_file("../roms/test.c8");
    match rom_code {
        Ok(_) => println!("Ok"),
        Err(_) => println!("Err")
    };
}

fn load_file(file_path: &str) -> Result<Vec<u8>, io::Error> {
    let file_read_result = std::fs::read(file_path);
    match file_read_result {
        Ok(bytes) => Ok(bytes),
        Err(error) => Err(error),
    }
}