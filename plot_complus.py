"""
    Plot the output log file from mon_complus.vbs.  The valid file
    extension for saving the plot to a file are .jpg, .pdf, .png,
    and .svg.

Assumptions:
1)  Each log file must have a header line at the top of the file.
2)  The columns in the log file must begin in order from left with:
    Time, Computer Name, Process Name
3)  The fields in the log file must be separated by comma (,)

Syntax:
    %prog log[,log2,...] [process1,process2,...] [options]

Examples:
1)  Show all processes in a log (generally not too useful):
    %prog log1.txt

2)  Show a process from 2 different logs on 1 graph:
    %prog log1.txt,log2.txt process1

3)  Same as above, but save it to a file:
    %prog log1.txt,log2.txt process1 -o graph.png

4)  Show 2 processes from a log on 1 graph:
    %prog log1.txt process1,process2

5)  Same as above, but save it to a file:
    %prog log1.txt process1,process2 -o graph.png

6)  Save a graph for each process in a log to a folder:
    %prog log1.txt -a graph_folder


Requirement:
    Python 2.4+, matplotlib, Numpy
"""

# Ie, what y-axis do you want in the output graph?
# Choices of MemorySeries and DefaultSeries.
SERIES_WE_ARE_INTERESTED_IN = [
    ("%ProcessorTime", "DefaultSeries"),
    ("PrivateBytes",   "MemorySeries"),
]


USAGE = __doc__
VERSION = "1.3"
DEFAULT_EXT = ".png"
VALID_EXTS = (DEFAULT_EXT, ".jpg", ".pdf", ".svg")


# BUGS:
# 1.    Save file is not in "landscape" (wait for upstream (matplotlib) to fix)
# 2.    When creating a lot of graphs using "-a", it might fail with error:
#       Fail to create pixmap with Tk_GetPixmap in ImgPhotoInstanceSetSize.
#       This is a Tk bug upstream.  A temporary solution when encounter this
#       is to set the value of the GDIProcessHandleQuota in
#       HKEY_LOCAL_MACHINE/SOFTWARE/Microsoft/Windows NT/Windows to a higher
#       value.
# 3.    It currently use a lot of RAM (with a possible leak); To minimize the
#       use of RAM, we can optimize by using array.array on number series;
#       use __slots__ on process objects...etc.


import sys, os, csv, random, re, optparse
from time import strptime, mktime
import pylab as g


class DefaultSeries:
    marker = "-"
    unit   = ""
    def __init__(self, name):
        self.name   = name
        self.index  = None

    def transform(self, value):
        """
        Behaviour:
            >>> s = DefaultSeries("%ProcessorTime")
            >>> s.transform('10')
            10
            >>> s.transform('')
            0
            >>> s.transform(None)
            0
        """
        try:
            return int(value)
        except:
            return 0 # Oh well, best compromise we can do.


class MemorySeries(DefaultSeries):
    unit = "(MB)"
    def transform(self, value):
        """
        Display value in MBytes instead of Bytes.

        Behaviour:
            >>> s = MemorySeries("PrivateBytes")
            >>> s.transform(12345678) # 12 MB
            12.345677999999999
            >>> s.transform(0)
            0.0
            >>> s.transform('')
            0
        """
        try:
            return int(value) / 1000000.0
        except:
            return 0 # Oh well, best compromise we can do.



def random_colour_generator():
    """
    This function returns a generator object
    for random colours.  The generator object
    will produce an infinite stream of colours.

    Behaviour:
        >>> COLOURS = random_colour_generator()
        >>> colour1 = COLOURS.next()
        >>> colour2 = COLOURS.next()
        >>> colour3 = COLOURS.next()
        >>> colour1 != colour2 != colour3
        True
    """
    # We are listing the colours to be used and order here
    # since it is hard to randomize a colour sequence that
    # is easy to distinguish.
    colours = [
        'aqua',
        'black',
        'blue',
        'blueviolet',
        'brown',
        'cadetblue',
        'chartreuse',
        'chocolate',
        'coral',
        'cornflowerblue',
        'crimson',
        'deeppink',
        'dimgray',
        'dodgerblue',
        'firebrick',
        'forestgreen',
        'gold',
        'goldenrod',
        'gray',
        'green',
        'greenyellow',
        'hotpink',
        'indianred',
        'indigo',
        'khaki',
        'lawngreen',
        'lime',
        'limegreen',
        'magenta',
        'maroon',
        'midnightblue',
        'navy',
        'olive',
        'olivedrab',
        'orange',
        'orangered',
        'orchid',
        'palegreen',
        'palevioletred',
        'peachpuff',
        'peru',
        'pink',
        'plum',
        'purple',
        'red',
        'rosybrown',
        'royalblue',
        'saddlebrown',
        'salmon',
        'sandybrown',
        'seagreen',
        'silver',
        'skyblue',
        'slateblue',
        'slategray',
        'springgreen',
        'steelblue',
        'tan',
        'teal',
        'tomato',
        'turquoise',
        'violet',
        'yellow',
        'yellowgreen']
    length = len(colours)
    counter = 0
    random.shuffle(colours)

    while 1:
        yield colours[counter]
        counter += 1
        if counter >= length:
            counter = 0
            random.shuffle(colours)


# Global instance -- Use it like this: COLOURS.next()
COLOURS = random_colour_generator()


class UsageError(Exception):
    def __init__(self, msg=""):
        self.msg = msg


class SeriesFilter:
    """
    Verify the series/columns/metrics the user is
    interested in do exist in the log file, and
    then capture their positions.

    Behaviour:
        >>> user_preference = [
        ... ("%ProcessorTime", "DefaultSeries"),
        ... ("PrivateBytes",   "MemorySeries"),
        ... ]
        >>> header = ['Time','CN','PN','%ProcessorTime','%UT','TC','PrivateBytes','WS']
        >>> f = SeriesFilter(user_preference)
        >>> f.initialize(header)
        >>> len(f.series)
        2
        >>> f.series[0].name, f.series[0].index
        ('%ProcessorTime', 3)
        >>> f.series[1].name, f.series[1].index
        ('PrivateBytes', 6)

    It should not throw any error if one of the columns match:
        >>> header = ['Time','','PrivateBytes','','','','']
        >>> f = SeriesFilter(user_preference)
        >>> f.initialize(header)
        >>>

    It should throw an error if no columns match:
        >>> header = ['Time','','','','','','']
        >>> f = SeriesFilter(user_preference)
        >>> f.initialize(header)
        Traceback (most recent call last):
        ...
            raise UsageError("Desired columns not found in log files!")
        UsageError: <unprintable instance object>
    """
    def __init__(self, user_preference):
        """
        user_preference -- a list of (column_name, series_class),
                           containing the series/columns the user
                           is interested in.
        """
        self.user_desired_series = user_preference
        self.series = []

    def initialize(self, header):
        """
        Initialize the filter with a header row.  Will
        raise an exception if no columns match at all!

        header -- a list of string, each component a column name.
        """
        header_length = len(header)
        for name, cls_name in self.user_desired_series:

            for i in range(header_length):
                if name == header[i]:
                    c = globals()[cls_name](name)
                    c.index = i
                    self.series.append(c)
                    break

        if len(self.series) == 0:
            raise UsageError("Desired columns not found in log files!")


class ProcessFilter:
    """
    This class identifies the processes the
    user is interested in when parsing a
    log file.

    Behaviour:
        >>> f = ProcessFilter("session")
        >>> "RBCWSSession" in f
        True
        >>> "Some Session Server" in f
        True
        >>> "Quake" in f
        False
        >>> "" in f
        False
        >>> f = ProcessFilter("UserInfo,Session")
        >>> "RBCWSUserInfo" in f
        True
        >>> "Some Session Server" in f
        True
        >>> "Some Userinfo App" in f
        True
        >>> "Quake" in f
        False
        >>> "" in f
        False
    """
    def __init__(self, name_patterns):
        """
        name_patterns -- a string of process name patterns
                         separated by comma that the user
                         is interested in.
        """
        patterns = ["(%s)+" % p for p in name_patterns.split(",")]
        self._regex = re.compile("|".join(patterns), re.IGNORECASE)

    def __contains__(self, process_name):
        """
        Return True if process_name is one of the processes
        the user is interested in, False otherwise.
        """
        if self._regex.search(process_name) is None:
            return False
        return True


class AllProcesses:
    """
    This process filter represents all processes,
    hence always return True.

    Behaviour:
        >>> f = AllProcesses()
        >>> "RBCWSSession" in f
        True
        >>> "Quake" in f
        True
        >>> "" in f
        True
    """
    def __contains__(self, val):
        return True


class Process:
    """
    A representation of a process.  Contains its identity,
    the time series and other column/metrics series.

    Behaviour:
        >>> p = Process("RBCWSSession(1234)", "test_log.txt")
        >>> p.process_name, p.process_id, p.id
        ('RBCWSSession', '1234', 'RBCWSSession(1234) test_log.txt')

        >>> p.set("PrivateBytes", "1000")
        >>> p.set("PrivateBytes", "2000")
        >>> p.set("PrivateBytes", "3000")
        >>> p.get("PrivateBytes")
        ['1000', '2000', '3000']

        >>> p.set("%ProcessorTime", "40")
        >>> p.get("%ProcessorTime")
        ['40']
        >>> p.get("PrivateBytes")
        ['1000', '2000', '3000']

    An abnormal case here. It does happen sometimes:
        >>> p = Process("()", "test_log.txt")
        >>> p.process_name, p.process_id, p.id
        ('', '', '() test_log.txt')
    """

    _pattern = re.compile("[()]")

    def __init__(self, process_name_and_id, log_filename):
        """
        process_name_and_id -- a string in the format of
                               ProcessName(ProcessId).
        log_filename -- a string of the filename (no path).
        """
        self.process_name, self.process_id, dummy = \
                self._pattern.split(process_name_and_id, maxsplit=2)
        self.log_filename   = log_filename
        self.id             = Process.compose_id(process_name_and_id, log_filename)
        self.time_series    = []

    def get(self, series):
        """
        Return a series.
        """
        return getattr(self, series)

    def set(self, series, value):
        """
        Append a value to a particular series.

        series -- a string of the series name.
        """
        val_list = getattr(self, series, None)
        if val_list is None:
            val_list = []
            setattr(self, series, val_list)
        val_list.append(value)

    @staticmethod
    def compose_id(process_name_and_id, log_filename):
        """
        Return the ID that is used to identify a
        particular process from a log file.
        """
        return "%s %s" % (process_name_and_id, log_filename)


class LogParser:
    r"""
    Parse mon_complus.vbs output log files.

    A smoke test:
        >>> # Setting up the test.
        >>> user_preference = [
        ... ("%ProcessorTime", "DefaultSeries"),
        ... ("PrivateBytes", "MemorySeries"),
        ... ]
        >>> sf = SeriesFilter(user_preference)
        >>> pf = ProcessFilter("session,userinfo")
        >>>
        >>> # Setting up the test log file.
        >>> content = ['Microsoft (R) Windows Script Host Version 5.6\n',
        ... 'Copyright (C) Microsoft Corporation 1996-2001. All rights reserved.\n',
        ... '\n',
        ... 'Time,CN,PN(ID),%ProcessorTime,%UT,TC,PrivateBytes,WS\n',
        ... '4/3/2007 10:00:37 AM,.,Idle(0),100,0,8,0,16384\n',
        ... '4/3/2007 10:00:37 AM,.,System(4),0,0,110,28672,28672\n',
        ... '4/3/2007 10:00:37 AM,.,RBCWSSession(6520),23,0,31,30208000,37974016\n',
        ... ',,,,,,,\n'
        ... '4/3/2007 10:00:42 AM,.,RBCWSSession(6520),24,0,31,25071616,34078720\n',
        ... '4/3/2007 10:00:42 AM,.,RBCWSUserInfo(10496),11,0,32,13414400,21475328\n',
        ... '4/4/2007,.,RBCWSSession(6520),25,0,31,25214976,34222080\n',
        ... ]
        >>> from tempfile import mkstemp
        >>> import os
        >>> fd, log_path = mkstemp()
        >>> log_file = os.fdopen(fd, 'w')
        >>> log_file.writelines(content)
        >>> log_file.close()
        >>>
        >>> # Now we run the smoke test of parsing 1 log file containing
        >>> # 2 processes (RBCWSSession, RBCWSUserInfo) we are interested,
        >>> # with the 2 metrics (above) we are interested in.
        >>> parser = LogParser(pf, sf)
        >>> processes = parser.parse_logs(log_path)
        >>> len(processes) == 2
        True
        >>> session = processes[0]
        >>> session.process_name
        'RBCWSSession'
        >>> len(session.time_series) == 3
        True
        >>> session.get("%ProcessorTime")
        [23, 24, 25]
        >>> session.get("PrivateBytes")
        [30.207999999999998, 25.071615999999999, 25.214976]
        >>> userinfo = processes[1]
        >>> userinfo.process_name
        'RBCWSUserInfo'
        >>> len(userinfo.time_series) == 1
        True
        >>> userinfo.get("%ProcessorTime")
        [11]
        >>> userinfo.get("PrivateBytes")
        [13.414400000000001]
        >>>
        >>> # Clean up.
        >>> os.remove(log_path)
    """
    def __init__(self, process_filter, series_filter):
        """
        process_filter -- a ProcessFilter instance that
                          is used to capture only those
                          processes the user is interested
                          in.
        series_filter -- a SeriesFilter instance that is
                         used to capture only those
                         series/metrics/columns that the
                         user is interested in.

        """
        self.process_filter = process_filter
        self.series_filter = series_filter
        self.are_headers_verified = False

    def parse_logs(self, raw_paths):
        """
        Parse all mon_complus.vbs output log files given.
        Return a sorted process list with all the metrics.

        raw_paths -- a string of output log files separated by comma.
        """
        paths = [os.path.abspath(p) for p in raw_paths.split(",")]
        processes = {}

        # We parse every log file here.
        for path in paths:
            if not os.path.exists(path):
                print "<%s> does not exist!" % path
                continue

            partial = self._get_processes_in_log(path)
            processes.update(partial)

        # Sort it in proper order for easy viewing.
        return sorted(processes.values(),
                key=lambda p: "%s %s %s" % (
                p.process_name, p.log_filename, p.process_id))

    def _get_processes_in_log(self, path):
        """
        Return the processes in a log file as a dict.
        """
        processes = {}
        filename = os.path.basename(path)
        log_file = csv.reader(open(path, "r"))

        for line in log_file:
            if len(line) < 3:
                # An empty line or not all column presents so we skip it.
                pass
            elif line[0] == "Time":
                # We only care about the header line from the first log
                # file, assuming all log files would have the same columns.
                if not self.are_headers_verified:
                    self.series_filter.initialize(line)
                    self.are_headers_verified = True
            else:
                name_n_id = line[2]
                if name_n_id in self.process_filter:
                    id = Process.compose_id(name_n_id, filename)
                    p = processes.setdefault(id, Process(name_n_id, filename))

                    time = line[0]
                    if len(time) > 0:
                        # Only record the stat if time is not empty!
                        p.time_series.append(time)
                        for s in self.series_filter.series:
                            p.set(s.name, s.transform(line[s.index]))

        return processes


def make_time_series(series):
    """
    Make a elapsed time series by converting from
    a timestamp series.  Return a list of float of
    elapsed time in minutes.

    series -- a list of string of timestamp

    Behaviour:
        >>> time_series = [
        ... '4/3/2007 10:00:37 AM',
        ... '4/3/2007 12:00:00 PM',
        ... '4/3/2007 11:59:57 PM',
        ... '4/4/2007',
        ... ]
        >>> from plot_complus import make_time_series
        >>> make_time_series(time_series)
        [0.0, 119.38333333333334, 839.33333333333337, 839.38333333333333]
    """
    def get_sec(timestamp):
        try:
            return mktime(strptime(timestamp, "%m/%d/%Y %I:%M:%S %p"))
        except ValueError:
            # If the time is on the dot (00:00:00),
            # only date will be shown.
            return mktime(strptime(timestamp, "%m/%d/%Y"))

    zero = get_sec(series[0])
    return [(get_sec(t) - zero) / 60 for t in series]


class Sizer:
    """
    This class calculates the size and location of the
    mini-graphs (each series is a mini-graph) in a page
    (ie, a graph).

    Behaviour:
        >>> s = Sizer(1)
        >>> s.coordinates(0)
        [0.10000000000000001, 0.074999999999999997, 0.59999999999999998, 0.84999999999999998]

        >>> s = Sizer(2)
        >>> s.coordinates(0)
        [0.10000000000000001, 0.5, 0.59999999999999998, 0.42499999999999999]
        >>> s.coordinates(1)
        [0.10000000000000001, 0.074999999999999997, 0.59999999999999998, 0.42499999999999999]

        >>> s = Sizer(3)
        >>> s.coordinates(0)
        [0.10000000000000001, 0.6409999999999999, 0.59999999999999998, 0.28299999999999997]
        >>> s.coordinates(1)
        [0.10000000000000001, 0.35799999999999998, 0.59999999999999998, 0.28299999999999997]
        >>> s.coordinates(2)
        [0.10000000000000001, 0.074999999999999997, 0.59999999999999998, 0.28299999999999997]
    """
    def __init__(self, num_of_graphs):
        self.highest_id = num_of_graphs - 1
        self.inc =round((1.0 - 0.075 * 2) / num_of_graphs, 3)

    def coordinates(self, graph_id):
        """
        Return a list of coordinates, in this format:
        [left, bottom, width, height]
        """
        return [0.1, (self.highest_id - graph_id) * self.inc + 0.075, 0.85, self.inc]


def plot_graph(processes, series_filter, save_file_path=None):
    """
    Create a graph.  Either display the graph or save the
    graph. This function will only create ONE graph.

    processes -- a list of Process instance.
    save_file_path -- If not None, save the graph to the file path
                      specified by this parameter (a string).
    """
    g.figure(figsize=(12, 7))

    handles = []
    labels = []
    sizer = Sizer(len(series_filter.series))

    for p in processes:
        time_series = make_time_series(p.time_series)
        colour = COLOURS.next()
        print p.id, colour

        for count, series in enumerate(series_filter.series):
            ax = g.axes(sizer.coordinates(count))
            try:
                h = g.plot(time_series, p.get(series.name),
                        series.marker, color=colour, label=p.id)
            except:
                print "Error in generating graph for %s of %s!" \
                        % (p.id, series.name)
                raise

            g.ylabel("%s %s" % (series.name, series.unit))
            g.grid(True)
            g.setp(ax.get_xticklabels(), visible=False)

        handles.append(h)
        labels.append(p.id)

    g.setp(ax.get_xticklabels(), visible=True)
    g.xlabel("Elapsed Time (min)")
    legend = g.figlegend(handles, labels, 'lower right',
            pad=0.1, labelsep=0.0025, handlelen=0.025,
            handletextsep=0.01, axespad=0.08)

    for t in legend.get_texts():
        t.set_fontsize(8)

    if save_file_path != None:
        g.savefig(save_file_path, orientation='landscape')
        g.close("all")
    else:
        g.show()


def run_test_and_exit():
    import doctest
    doctest.testmod(verbose=True)
    sys.exit(0)


def main():
    try:
        # Parse and validate commandline options and arguments.
        p = optparse.OptionParser(USAGE)
        p.add_option("-o", "--output", dest="save_file_path",
                help="Save the graph to a file instead of displaying it.")
        p.add_option("-a", "--for-each-app", dest="save_dir_path",
                help="Generate a graph for each COM+ application and save them.")
        (options, args) = p.parse_args()

        args_num = len(args)
        if args_num < 1:
            raise UsageError("Wrong number of input arguments!")

        if options.save_file_path and options.save_dir_path:
            raise UsageError("-o and -a cannot be used together!" \
                    " Please use either one.")

        if args_num == 2 and options.save_dir_path:
            raise UsageError("-a cannot be used with the patterns argument!" \
                    " Please use either one.")

        if args[0] == "DOCTEST":
            run_test_and_exit()
        else:
            data_paths = args[0]

        if args_num == 2:
            process_filter = ProcessFilter(args[1])
        else:
            process_filter = AllProcesses()

        series_filter = SeriesFilter(SERIES_WE_ARE_INTERESTED_IN)
        logparser = LogParser(process_filter, series_filter)
        processes = logparser.parse_logs(data_paths)

        if len(processes) == 0:
            # There is nothing for us to plot, raise error.
            raise UsageError("No process matches the given patterns!")

        # The -a option: Generate plots for all processes and
        # save them in a folder.
        if options.save_dir_path:

            # Making sure the folder is reachable.  If not, create it.
            dir_path = os.path.abspath(options.save_dir_path)
            if not os.path.exists(dir_path):
                os.makedirs(dir_path)
            else:
                if not os.path.isdir(dir_path):
                    raise UsageError("%s is not a directory!" % dir_path)

            # Here we group all processes from all log files with the
            # same process name together, then we will generate
            # the graph for each group. process_groups is a dict of list.
            process_groups = {}
            for p in processes:
                if p.process_name not in process_groups:
                    process_groups[p.process_name] = []
                process_groups[p.process_name].append(p)

            for name, p in process_groups.iteritems():
                save_path = os.path.join(dir_path, name + DEFAULT_EXT)
                plot_graph(p, series_filter, save_file_path=save_path)

        # The -o option: Generate a plot and save it to a file.
        elif options.save_file_path:
            save_path = os.path.abspath(options.save_file_path)

            # Making sure the path is reachable.
            dir_path = os.path.dirname(save_path)
            if not os.path.isdir(dir_path):
                raise UsageError("%s is not a valid directory!" % dir_path)

            # Do a file extension check.
            path_without_ext, ext = os.path.splitext(save_path)
            if len(ext) > 0:
                # Set to default extension if the specified one
                # is invalid.
                if ext.lower() not in VALID_EXTS:
                    print "The graph cannot be saved in %s!  " \
                          "Will save it in %s instead." % (ext, DEFAULT_EXT)
                    save_path = path_without_ext + DEFAULT_EXT
            else:
                # No extension specified, use the default.
                print "The graph will be saved in %s." % DEFAULT_EXT
                save_path = path_without_ext + DEFAULT_EXT

            plot_graph(processes, series_filter, save_file_path=save_path)

        # The interactive option (default): Generate a plot and show it.
        else:
            plot_graph(processes, series_filter)

        sys.exit(0)

    except UsageError, err:
        print >> sys.stderr, err.msg
        sys.exit(1)


if __name__ == "__main__":
    main()
