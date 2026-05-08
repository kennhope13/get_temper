import requests
import sys
import xml.etree.ElementTree as ET
import csv
import os
import time
from datetime import datetime
from requests.auth import HTTPDigestAuth

# Cấu hình stdout cho tiếng Việt
try:
    sys.stdout.reconfigure(encoding='utf-8')
except AttributeError:
    pass

# Thông tin Camera
IP = "192.168.10.153"
PORT = 80
USER = "admin"
PASS = "Demo@2024"
CSV_FILE = "camera_metrics.csv"

CSV_COLUMNS = ['dateTime', 'audioDecibel', 'frequency']

# Bộ nhớ đệm giữ dữ liệu đang chờ gộp
pending_data = {}
last_update_time = 0

def format_hik_time(hik_time_str):
    """Chuyển đổi 2026-04-27T21:04:19+07:00 thành 27-04-2026 21:04:19"""
    try:
        # Lấy 19 ký tự đầu: YYYY-MM-DDTHH:MM:SS
        clean_time = hik_time_str[:19]
        dt = datetime.strptime(clean_time, "%Y-%m-%dT%H:%M:%S")
        return dt.strftime("%d-%m-%Y %H:%M:%S")
    except:
        return hik_time_str

def save_to_csv(data):
    """Ghi dữ liệu vào CSV với định dạng thời gian mới"""
    if not data: return

    has_db = data.get('audioDecibel')
    has_fr = data.get('frequency')
    
    if has_db and has_fr:
        # Định dạng lại thời gian trước khi lưu
        data['dateTime'] = format_hik_time(data.get('dateTime', ''))
        
        file_exists = os.path.isfile(CSV_FILE)
        try:
            with open(CSV_FILE, 'a', newline='', encoding='utf-8') as f:
                writer = csv.DictWriter(f, fieldnames=CSV_COLUMNS, extrasaction='ignore')
                if not file_exists or os.stat(CSV_FILE).st_size == 0:
                    writer.writeheader()
                writer.writerow(data)
            
            print(f"[{data['dateTime']}] 🏆 ĐÃ LƯU: {has_db} dB | {has_fr} Hz")
        except Exception as e:
            print(f"Lỗi ghi file: {e}")
    
    # Xóa trắng buffer sau khi ghi hoặc sau khi quá hạn
    return

def process_xml(xml_data):
    global pending_data, last_update_time
    try:
        root = ET.fromstring(xml_data)
        new_packet = {}
        for el in root.iter():
            tag = el.tag.split('}', 1)[1] if '}' in el.tag else el.tag
            if el.text and el.text.strip():
                new_packet[tag] = el.text.strip()
        
        if not new_packet: return

        now = time.time()
        
        if pending_data:
            time_passed = now - last_update_time
            is_duplicate = ('audioDecibel' in new_packet and 'audioDecibel' in pending_data) or \
                           ('frequency' in new_packet and 'frequency' in pending_data)
            
            if time_passed > 10.0 or is_duplicate:
                save_to_csv(pending_data)
                pending_data = {}

        if not pending_data:
            pending_data = new_packet
        else:
            for k, v in new_packet.items():
                if k in CSV_COLUMNS:
                    pending_data[k] = v
        
        last_update_time = now

        if 'audioDecibel' in pending_data and 'frequency' in pending_data:
            save_to_csv(pending_data)
            pending_data = {}

    except Exception:
        pass

def start_monitoring():
    url = f"http://{IP}:{PORT}/ISAPI/Event/notification/alertStream"
    print(f"🚀 Hệ thống giám sát (Định dạng thời gian: DD-MM-YYYY)")
    print(f"📌 Dữ liệu đang được ghi vào {CSV_FILE}...")

    try:
        response = requests.get(url, auth=HTTPDigestAuth(USER, PASS), stream=True, timeout=None)
        if response.status_code != 200:
            print(f"❌ Lỗi: {response.status_code}")
            return

        xml_buffer = ""
        for line in response.iter_lines():
            if line:
                decoded_line = line.decode('utf-8', 'ignore')
                if "<EventNotificationAlert" in decoded_line:
                    xml_buffer = decoded_line
                else:
                    xml_buffer += decoded_line
                
                if "</EventNotificationAlert>" in decoded_line:
                    process_xml(xml_buffer)
                    xml_buffer = ""

    except KeyboardInterrupt:
        if pending_data: save_to_csv(pending_data)
        print("\n👋 Đã dừng.")
    except Exception as e:
        print(f"⚠️ Lỗi: {e}")

if __name__ == "__main__":
    start_monitoring()
