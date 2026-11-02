# Searches freesound.org's public site for placeholder sounds (ADR 0036): CC0 only, most downloaded first, one line per
# result with its id, length, downloads, rating (count), uploader and title. Open a candidate's page
# (https://freesound.org/s/<id>/) to read its description before picking it: the rip rule (decision 427) refuses any
# upload that names or imitates a commercial game or film.
#
#   perl .claude/scripts/find-freesound.pl <longest seconds> <search words...>
#   perl .claude/scripts/find-freesound.pl 1 sword swing
#
# Searching too fast gets an HTTP 429 from freesound: leave a few seconds between searches.
use strict; use warnings;

my ($max, @words) = @ARGV;
die "usage: perl find-freesound.pl <longest seconds> <search words...>\n" unless defined $max && @words;
my $query = join('+', @words);
my $url = "https://freesound.org/search/?q=$query&f=license:%22Creative+Commons+0%22&s=Downloads+(most+first)";
my $html = `curl -s -L -A "Mozilla/5.0" "$url"`;

my @results = split(/data-sound-id="/, $html);
shift @results;
my %seen;
for my $result (@results) {
  my ($id) = $result =~ /^(\d+)/;
  next if !defined $id || $seen{$id}++;
  my ($seconds) = $result =~ /data-duration="([\d.]+)"/;
  my ($downloads) = $result =~ /data-num-downloads="(\d+)"/;
  my ($rating) = $result =~ /Average rating of ([\d.]+)/;
  my ($ratings) = $result =~ /bw-rating__count[^>]*>\((\d+)\)/;
  my ($author, $title) = $result =~ m{href="/people/([^/]+)/sounds/\d+/" title="([^"]*)"};
  next unless defined $seconds && defined $title && $seconds <= $max;
  printf "%-7s %5.2fs %6s dl  %s(%s)  %-18s %s\n", $id, $seconds, $downloads // '?', $rating // '-', $ratings // 0,
    substr($author, 0, 18), substr($title, 0, 60);
}
