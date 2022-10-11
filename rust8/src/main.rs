use std::fs::File;
use std::io::Read;

fn main() {
    let mut pc: u16 = 0;
    let mut vreg: Vec<u8> = vec![0; 16];
    let mut ireg: u16 = 0;

    let mut memory: Vec<u8> = vec![0; 0x10000];
    let mut stack: Vec<u16> = vec![];

    let rom_code = load_file("../../assembler/roms/test.c8");
}

fn load_file(file_path: &str) -> Vec<u8> {

}