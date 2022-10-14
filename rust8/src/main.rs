use gtk::prelude::*;
use gtk::{Application, ApplicationWindow, Menu, MenuBar, MenuItem};

use std::fs::File;
use std::io;

fn main() {
    
    let app = Application::builder()
        .application_id("cbspace.chip8")
        .build();
    
    app.connect_activate(|app| {
        build_ui(&app);
    });
    
    run_emulator();
    app.run();
}

fn build_ui(app: &gtk::Application) {
    let window = ApplicationWindow::builder()
        .application(app)
        .default_width(320)
        .default_height(240)
        .title("Chip-8")
        .build();

    let v_box = gtk::Box::new(gtk::Orientation::Vertical, 10);
    let menu_bar = MenuBar::new();
    
    let file = MenuItem::with_label("File");
    let menu = Menu::new();
    let open = MenuItem::with_label("Open");
    menu.append(&open);
    file.set_submenu(Some(&menu));

    let about = MenuItem::with_label("About");

    menu_bar.append(&file);
    menu_bar.append(&about);
    v_box.pack_start(&menu_bar, false, false, 0);
    window.add(&v_box);

    window.show_all();
}

fn run_emulator() {
    let mut pc: u16 = 0x200;
    let mut vreg: Vec<u8> = vec![0; 16];
    let mut ireg: u16 = 0;

    let mut memory: Vec<u8> = vec![0; 0x10000];
    let mut stack: Vec<u16> = vec![];

    load_font_set(&mut memory);

    match load_file("../roms/test.c8", &mut memory) {
        Ok(()) => {},
        Err(error) => println!("Error: {}", error)
    };
}

fn load_file(file_path: &str, memory: &mut Vec<u8>) -> Result<(), io::Error> {
    let bytes = std::fs::read(file_path)?;
    let mut address = 0x200;
    for byte in bytes {
        memory[address] = byte;
        address += 1;
    }
    Ok(())
}

fn load_font_set(memory: &mut Vec<u8>) {
    let chip8_fontset: Vec<u8> = vec![
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    ];

    for (index, byte) in chip8_fontset.iter().enumerate() {
        memory[0x50 + index] = *byte;
    }
}

fn cpu_cycle() {

}