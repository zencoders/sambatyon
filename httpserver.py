import string, cgi, time
from os import curdir, sep
from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer

class Handler(BaseHTTPRequestHandler):
	def do_GET(self):
		try:
			f = open(curdir + sep + 'nodes.xml')
			self.send_response(200)
			self.send_header('Content-type', 'application/xml')
			self.end_headers()
			self.wfile.write(f.read())
			f.close()
			return
		except IOError:
			self.send_error(404, 'File not found!')

def main():
	try:
		server = HTTPServer(('',8000), Handler)
		print 'started httpserver....'
		server.serve_forever()
	except KeyboardInterrupt:
		print '^C received, shutting down'
		server.socket.close()

if __name__ == '__main__':
	main()