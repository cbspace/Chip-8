use gtk::prelude::*;
use gtk::{Application, ApplicationWindow, Menu, MenuBar, MenuItem, Orientation};
use glib::clone;

use std::fs::File;
use std::io;

fn main() {
    
    let app = Application::builder()
        .application_id("cbspace.chip8")
        .build();
    
    app.connect_activate(|app| {
        build_ui(&app);
    });
    
    app.run();
}

fn build_ui(app: &gtk::Application) {
    
    let gtk_box = gtk::Box::builder()
        .orientation(Orientation::Vertical)
        .build();

    let window = ApplicationWindow::builder()
        .application(app)
        .default_width(320)
        .default_height(240)
        .title("Chip-8")
        .child(&gtk_box)
        .build();
    
    let menu_bar = MenuBar::new();
    
    let emulator_menu = MenuItem::with_label("Emulator");
    let about_menu = MenuItem::with_label("About");

    let emulator_sub_menu = Menu::new();
    let open_menu_item = MenuItem::with_label("Open ROM");
    let quit_menu_item = MenuItem::with_label("Quit");
    emulator_sub_menu.append(&open_menu_item);
    emulator_sub_menu.append(&quit_menu_item);

    open_menu_item.connect_activate(move |_| {
        run_emulator();
    });

    quit_menu_item.connect_activate(clone!(@weak window => move |_| {
        window.close();
    }));

    emulator_menu.set_submenu(Some(&emulator_sub_menu));
    menu_bar.append(&emulator_menu);
    menu_bar.append(&about_menu);

    gtk_box.pack_start(&menu_bar, false, false, 0);
    window.show_all();
}

fn run_emulator() {
    let mut pc: u16 = 0x200;
    let mut vreg: Vec<u8> = vec![0; 16];
    let mut ireg: u16 = 0;

    let mut memory: Vec<u8> = vec![0; 0x10000];
    let mut stack: Vec<u16> = vec![];

    load_font_set(&mut memory);

    let file_path = "../roms/test.c8";
    match load_file(file_path, &mut memory) {
        Ok(()) => println!("Loaded ROM: {}", file_path),
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